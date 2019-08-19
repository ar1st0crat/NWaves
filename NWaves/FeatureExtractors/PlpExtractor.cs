using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Perceptual Linear Predictive Coefficients extractor (PLP-RASTA)
    /// </summary>
    public class PlpExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "plp0", "plp1", "plp2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "plp" + i).ToList();

        /// <summary>
        /// Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)].
        /// By default it's bark filterbank like in original H.Hermansky's work
        /// (although many people prefer mel bands).
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// Filterbank center frequencies
        /// </summary>
        protected readonly double[] _centerFrequencies;

        /// <summary>
        /// Lower frequency
        /// </summary>
        protected readonly double _lowFreq;

        /// <summary>
        /// Upper frequency
        /// </summary>
        protected readonly double _highFreq;

        /// <summary>
        /// RASTA coefficient (if zero, then no RASTA filtering)
        /// </summary>
        protected readonly double _rasta;

        /// <summary>
        /// RASTA filters for each critical band
        /// </summary>
        protected readonly RastaFilter[] _rastaFilters;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        protected readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients
        /// </summary>
        protected readonly float[] _lifterCoeffs;

        /// <summary>
        /// FFT transformer
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a signal spectrum grouped to frequency bands
        /// </summary>
        protected readonly float[] _bandSpectrum;

        /// <summary>
        /// Equal loudness weighting coefficients
        /// </summary>
        protected readonly double[] _equalLoudnessCurve;

        /// <summary>
        /// LPC order
        /// </summary>
        protected readonly int _lpcOrder;

        /// <summary>
        /// Internal buffer for LPC-coefficients
        /// </summary>
        protected readonly float[] _lpc;

        /// <summary>
        /// Precomputed IDFT table
        /// </summary>
        protected readonly float[][] _idftTable;

        /// <summary>
        /// Autocorrelation samples (computed as IDFT of power spectrum)
        /// </summary>
        protected readonly float[] _cc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="lpcOrder"></param>
        /// <param name="rasta"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="fftSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        /// <param name="filterbank"></param>
        /// <param name="centerFrequencies"></param>
        public PlpExtractor(int samplingRate,
                            int featureCount,
                            double frameDuration = 0.0256/*sec*/,
                            double hopDuration = 0.010/*sec*/,
                            int lpcOrder = 0,                       // will be autocalculated as featureCount - 1
                            double rasta = 0,
                            int filterbankSize = 24,
                            double lowFreq = 0,
                            double highFreq = 0,
                            int fftSize = 0,
                            int lifterSize = 0,
                            double preEmphasis = 0,
                            WindowTypes window = WindowTypes.Hamming,
                            float[][] filterbank = null,
                            double[] centerFrequencies = null)
            
            : base(samplingRate, frameDuration, hopDuration, preEmphasis, window)
        {
            FeatureCount = featureCount;

            // ================================ Prepare filter bank and center frequencies: ===========================================

            _lowFreq = lowFreq;
            _highFreq = highFreq;

            if (filterbank == null)
            {
                _blockSize = fftSize > FrameSize ? fftSize : MathUtils.NextPowerOfTwo(FrameSize);

                var barkBands = FilterBanks.BarkBandsSlaney(filterbankSize, samplingRate, _lowFreq, _highFreq);
                FilterBank = FilterBanks.BarkBankSlaney(filterbankSize, _blockSize, samplingRate, _lowFreq, _highFreq);

                _centerFrequencies = barkBands.Select(b => b.Item2).ToArray();
            }
            else
            {
                FilterBank = filterbank;
                filterbankSize = filterbank.Length;
                _blockSize = 2 * (filterbank[0].Length - 1);

                Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");

                if (centerFrequencies != null)
                {
                    _centerFrequencies = centerFrequencies;
                }
                else
                {
                    var herzResolution = (double)samplingRate / _blockSize;

                    // try to determine center frequencies automatically from filterbank weights:

                    _centerFrequencies = new double[filterbankSize];

                    for (var i = 0; i < filterbank.Length; i++)
                    {
                        var minPos = 0;
                        var maxPos = _blockSize / 2;

                        for (var j = 0; j < filterbank[i].Length; j++)
                        {
                            if (filterbank[i][j] > 0)
                            {
                                minPos = j;
                                break;
                            }
                        }
                        for (var j = minPos; j < filterbank[i].Length; j++)
                        {
                            if (filterbank[i][j] == 0)
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

            _rasta = rasta;

            if (rasta > 0)
            {
                _rastaFilters = Enumerable.Range(0, filterbankSize)
                                          .Select(f => new RastaFilter(rasta))
                                          .ToArray();
            }

            // ============== Precompute IDFT table for obtaining autocorrelation coeffs from power spectrum: =============

            _lpcOrder = lpcOrder > 0 ? lpcOrder : FeatureCount - 1;

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

            _lifterSize = lifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _spectrum = new float[_blockSize / 2 + 1];
            _bandSpectrum = new float[filterbankSize];
        }

        /// <summary>
        /// Standard method for computing PLP features.
        /// In each frame do:
        /// 
        ///     0) Apply window (base extractor does it)
        ///     1) Obtain power spectrum
        ///     2) Apply filterbank of bark bands (or mel bands)
        ///     3) [Optional] filter each component of the processed spectrum with a RASTA filter
        ///     4) Apply equal loudness curve
        ///     5) Take cubic root
        ///     6) Do LPC
        ///     7) Convert LPC to cepstrum
        ///     8) [Optional] lifter cepstrum
        /// 
        /// </summary>
        /// <param name="block">Samples for analysis</param>
        /// <param name="features">PLP vectors</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
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
        }

        /// <summary>
        /// Reset state
        /// </summary>
        public override void Reset()
        {
            if (_rastaFilters == null) return;

            foreach (var filter in _rastaFilters)
            {
                filter.Reset();
            }
        }

        /// <summary>
        /// In case of RASTA filtering computations can't be done in parallel
        /// </summary>
        /// <returns></returns>
        public override bool IsParallelizable() => _rasta == 0;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() => 
            new PlpExtractor( SamplingRate,
                              FeatureCount,
                              FrameDuration,
                              HopDuration,
                             _lpcOrder,
                             _rasta,
                              FilterBank.Length,
                             _lowFreq,
                             _highFreq,
                             _blockSize,
                             _lifterSize,
                             _preEmphasis,
                             _window,
                              FilterBank,
                             _centerFrequencies);
    }
}
