using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Filters;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Perceptual Linear Predictive Coefficients extractor (PLP-RASTA).
    /// </summary>
    public class PlpExtractor : FeatureExtractor
    {
        /// <summary>
        /// Feature names (simply "plp0", "plp1", "plp2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions
        {
            get
            {
                var names = Enumerable.Range(0, FeatureCount).Select(i => "plp" + i).ToList();
                if (_includeEnergy) names[0] = "log_En";
                return names;
            }
        }

        /// <summary>
        /// <para>Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)].</para>
        /// <para>
        /// By default it's bark filterbank like in original H.Hermansky's work 
        /// (although many people prefer mel bands).
        /// </para>
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// Filterbank center frequencies.
        /// </summary>
        protected readonly double[] _centerFrequencies;

        /// <summary>
        /// RASTA coefficient (if zero, then no RASTA filtering).
        /// </summary>
        protected readonly double _rasta;

        /// <summary>
        /// RASTA filters for each critical band.
        /// </summary>
        protected readonly RastaFilter[] _rastaFilters;

        /// <summary>
        /// Size of liftering window.
        /// </summary>
        protected readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients.
        /// </summary>
        protected readonly float[] _lifterCoeffs;

        /// <summary>
        /// Should the first PLP coefficient be replaced with LOG(energy).
        /// </summary>
        protected readonly bool _includeEnergy;

        /// <summary>
        /// Floor value for LOG-energy calculation.
        /// </summary>
        protected readonly float _logEnergyFloor;

        /// <summary>
        /// FFT transformer.
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step.
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a signal spectrum grouped to frequency bands.
        /// </summary>
        protected readonly float[] _bandSpectrum;

        /// <summary>
        /// Equal loudness weighting coefficients.
        /// </summary>
        protected readonly double[] _equalLoudnessCurve;

        /// <summary>
        /// LPC order.
        /// </summary>
        protected readonly int _lpcOrder;

        /// <summary>
        /// Internal buffer for LPC-coefficients.
        /// </summary>
        protected readonly float[] _lpc;

        /// <summary>
        /// Precomputed IDFT table.
        /// </summary>
        protected readonly float[][] _idftTable;

        /// <summary>
        /// Autocorrelation samples (computed as IDFT of power spectrum).
        /// </summary>
        protected readonly float[] _cc;

        /// <summary>
        /// Construct extractor from configuration options.
        /// </summary>
        /// <param name="options">Extractor configuration options</param>
        public PlpExtractor(PlpOptions options) : base(options)
        {
            FeatureCount = options.FeatureCount;

            // ================================ Prepare filter bank and center frequencies: ===========================================

            var filterbankSize = options.FilterBankSize;

            if (options.FilterBank is null)
            {
                _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);

                var low = options.LowFrequency;
                var high = options.HighFrequency;

                FilterBank = FilterBanks.BarkBankSlaney(filterbankSize, _blockSize, SamplingRate, low, high);

                var barkBands = FilterBanks.BarkBandsSlaney(filterbankSize, SamplingRate, low, high);
                _centerFrequencies = barkBands.Select(b => b.Item2).ToArray();
            }
            else
            {
                FilterBank = options.FilterBank;
                filterbankSize = FilterBank.Length;
                _blockSize = 2 * (FilterBank[0].Length - 1);

                Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");

                if (options.CenterFrequencies != null)
                {
                    _centerFrequencies = options.CenterFrequencies;
                }
                else
                {
                    var herzResolution = (double)SamplingRate / _blockSize;

                    // try to determine center frequencies automatically from filterbank weights:

                    _centerFrequencies = new double[filterbankSize];

                    for (var i = 0; i < FilterBank.Length; i++)
                    {
                        var minPos = 0;
                        var maxPos = _blockSize / 2;

                        for (var j = 0; j < FilterBank[i].Length; j++)
                        {
                            if (FilterBank[i][j] > 0)
                            {
                                minPos = j;
                                break;
                            }
                        }
                        for (var j = minPos; j < FilterBank[i].Length; j++)
                        {
                            if (FilterBank[i][j] == 0)
                            {
                                maxPos = j;
                                break;
                            }
                        }

                        _centerFrequencies[i] = herzResolution * (maxPos + minPos) / 2;
                    }
                }
            }

            // ==================================== Compute equal loudness curve: =========================================

            _equalLoudnessCurve = new double[filterbankSize];

            for (var i = 0; i < _centerFrequencies.Length; i++)
            {
                var level2 = _centerFrequencies[i] * _centerFrequencies[i];

                _equalLoudnessCurve[i] = Math.Pow(level2 / (level2 + 1.6e5), 2) * ((level2 + 1.44e6) / (level2 + 9.61e6));
            }

            // ============================== Prepare RASTA filters (if necessary): =======================================

            _rasta = options.Rasta;

            if (_rasta > 0)
            {
                _rastaFilters = Enumerable.Range(0, filterbankSize)
                                          .Select(f => new RastaFilter(_rasta))
                                          .ToArray();
            }

            // ============== Precompute IDFT table for obtaining autocorrelation coeffs from power spectrum: =============

            _lpcOrder = options.LpcOrder > 0 ? options.LpcOrder : FeatureCount - 1;

            _idftTable = new float[_lpcOrder + 1][];

            var bandCount = filterbankSize + 2;     // +2 duplicated edges
            var freq = Math.PI / (bandCount - 1);

            for (var i = 0; i < _idftTable.Length; i++)
            {
                _idftTable[i] = new float[bandCount];

                _idftTable[i][0] = 1.0f;

                for (var j = 1; j < bandCount - 1; j++)
                {
                    _idftTable[i][j] = 2 * (float)Math.Cos(freq * i * j);
                }

                _idftTable[i][bandCount - 1] = (float)Math.Cos(freq * i * (bandCount - 1));
            }

            _lpc = new float[_lpcOrder + 1];
            _cc = new float[bandCount];

            // =================================== Prepare everything else: ==============================

            _fft = new RealFft(_blockSize);

            _lifterSize = options.LifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _includeEnergy = options.IncludeEnergy;
            _logEnergyFloor = options.LogEnergyFloor;

            _spectrum = new float[_blockSize / 2 + 1];
            _bandSpectrum = new float[filterbankSize];
        }

        /// <summary>
        /// <para>Compute PLP-RASTA feature vector in one frame.</para>
        /// <para>
        /// General algorithm:
        /// <list type="number">
        ///     <item>Apply window</item>
        ///     <item>Obtain power spectrum</item>
        ///     <item>Apply filterbank of bark bands (or mel bands)</item>
        ///     <item>[Optional] filter each component of the processed spectrum with a RASTA filter</item>
        ///     <item>Apply equal loudness curve</item>
        ///     <item>Apply nonlinearity (take cubic root)</item>
        ///     <item>Do LPC</item>
        ///     <item>Convert LPC to cepstrum</item>
        ///     <item>[Optional] lifter cepstrum</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one PLP feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            // 0) base extractor applies window

            // 1) calculate power spectrum (without normalization)

            _fft.PowerSpectrum(block, _spectrum, false);

            // 2) apply filterbank on the result (bark frequencies by default)

            FilterBanks.Apply(FilterBank, _spectrum, _bandSpectrum);

            // 3) RASTA filtering in log-domain [optional]

            if (_rasta > 0)
            {
                for (var k = 0; k < _bandSpectrum.Length; k++)
                {
                    var log = (float)Math.Log(_bandSpectrum[k] + float.Epsilon);

                    log = _rastaFilters[k].Process(log);

                    _bandSpectrum[k] = (float)Math.Exp(log);
                }
            }

            // 4) and 5) apply equal loudness curve and take cubic root

            for (var k = 0; k < _bandSpectrum.Length; k++)
            {
                _bandSpectrum[k] = (float)Math.Pow(Math.Max(_bandSpectrum[k], 1.0) * _equalLoudnessCurve[k], 0.33);
            }

            // 6) LPC from power spectrum:

            var n = _idftTable[0].Length;

            // get autocorrelation samples from post-processed power spectrum (via IDFT):

            for (var k = 0; k < _idftTable.Length; k++)
            {
                var acc = _idftTable[k][0] * _bandSpectrum[0] +
                          _idftTable[k][n - 1] * _bandSpectrum[n - 3];  // add values at two duplicated edges right away

                for (var j = 1; j < n - 1; j++)
                {
                    acc += _idftTable[k][j] * _bandSpectrum[j - 1];
                }

                _cc[k] = acc / (2 * (n - 1));
            }

            // LPC:

            for (var k = 0; k < _lpc.Length; _lpc[k] = 0, k++) ;

            var err = Lpc.LevinsonDurbin(_cc, _lpc, _lpcOrder);

            // 7) compute LPCC coefficients from LPC

            Lpc.ToCepstrum(_lpc, err, features);


            // 8) (optional) liftering

            if (_lifterCoeffs != null)
            {
                features.ApplyWindow(_lifterCoeffs);
            }

            // 9) (optional) replace first coeff with log(energy) 

            if (_includeEnergy)
            {
                features[0] = (float)Math.Log(Math.Max(block.Sum(x => x * x), _logEnergyFloor));
            }
        }

        /// <summary>
        /// Reset extractor
        /// </summary>
        public override void Reset()
        {
            if (_rastaFilters is null) return;

            foreach (var filter in _rastaFilters)
            {
                filter.Reset();
            }
        }

        /// <summary>
        /// <para>Does the extractor support parallelization.</para>
        /// <para>
        /// Returns false in RASTA-filtering mode (i.e. if RASTA-coefficient is not 0). 
        /// Returns true in all other cases.
        /// </para>
        /// </summary>
        public override bool IsParallelizable() => _rasta == 0;

        /// <summary>
        /// Thread-safe copy of the extractor for parallel computations.
        /// </summary>
        public override FeatureExtractor ParallelCopy() => 
            new PlpExtractor(
                new PlpOptions
                {
                    SamplingRate = SamplingRate,
                    FeatureCount = FeatureCount,
                    FrameDuration = FrameDuration,
                    HopDuration = HopDuration,
                    LpcOrder = _lpcOrder,
                    Rasta = _rasta,
                    FilterBank = FilterBank,
                    FilterBankSize = FilterBank.Length,
                    FftSize = _blockSize,
                    LifterSize = _lifterSize,
                    PreEmphasis = _preEmphasis,
                    Window = _window,
                    CenterFrequencies = _centerFrequencies,
                    IncludeEnergy = _includeEnergy,
                    LogEnergyFloor = _logEnergyFloor
                });
    }
}
