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
        /// Number of filters in filterbank
        /// </summary>
        private readonly int _filterbankSize;

        /// <summary>
        /// Lower frequency
        /// </summary>
        private readonly double _lowFreq;

        /// <summary>
        /// Upper frequency
        /// </summary>
        private readonly double _highFreq;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// FFT transformer
        /// </summary>
        private readonly RealFft _fft;

        /// <summary>
        /// RASTA coefficient (if zero, then no RASTA filtering)
        /// </summary>
        private readonly double _rasta;

        /// <summary>
        /// RASTA filters for each critical band
        /// </summary>
        private readonly RastaFilter[] _rastaFilters;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        private readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients
        /// </summary>
        private readonly float[] _lifterCoeffs;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Window samples
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly float _preEmphasis;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        private readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a signal spectrum grouped to frequency bands
        /// </summary>
        private readonly float[] _bandSpectrum;

        /// <summary>
        /// Equal loudness weighting coefficients
        /// </summary>
        private readonly double[] _equalLoudnessCurve;

        /// <summary>
        /// LPC order
        /// </summary>
        private readonly int _lpcOrder;

        /// <summary>
        /// Internal buffer for LPC-coefficients
        /// </summary>
        private readonly float[] _lpc;

        /// <summary>
        /// Cross-correlation array (computed as IFFT of power spectrum)
        /// </summary>
        private readonly float[] _cc;

        /// <summary>
        /// Just a buffer for _cc imaginary parts in IFFT
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Fft transformer used for LPC
        /// </summary>
        private readonly RealFft _fftLpc;

        /// <summary>
        /// Internal buffer for a signal block at each step
        /// </summary>
        private readonly float[] _block;

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
        /// <param name="filterbank"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public PlpExtractor(int samplingRate,
                            int featureCount,
                            double frameDuration = 0.0256/*sec*/,
                            double hopDuration = 0.010/*sec*/,
                            int lpcOrder = 0,
                            double rasta = 0,
                            int filterbankSize = 20,
                            double lowFreq = 0,
                            double highFreq = 0,
                            int fftSize = 0,
                            float[][] filterbank = null,
                            int lifterSize = 0,
                            double preEmphasis = 0.0,
                            WindowTypes window = WindowTypes.Hamming) : 
            base(samplingRate, frameDuration, hopDuration)
        {
            FeatureCount = featureCount;

            _lowFreq = lowFreq;
            _highFreq = highFreq;

            double[] centerFrequencies;

            if (filterbank == null)
            {
                _fftSize = fftSize > FrameSize ? fftSize : MathUtils.NextPowerOfTwo(FrameSize);
                _filterbankSize = filterbankSize + 2;

                var barkBands = FilterBanks.Bark2Bands(_filterbankSize, _fftSize, SamplingRate, _lowFreq, _highFreq);
                FilterBank = FilterBanks.Triangular(_fftSize, SamplingRate, barkBands);

                centerFrequencies = barkBands.Select(b => b.Item2).ToArray();
            }
            else
            {
                _filterbankSize = filterbank.Length + 2;
                _fftSize = 2 * (filterbank[0].Length - 1);

                Guard.AgainstInvalidRange(FrameSize, _fftSize, "frame size", "FFT size");

                var herzResolution = (double)samplingRate / _fftSize;

                FilterBank = new float[_filterbankSize][];

                // determine center frequencies:

                centerFrequencies = new double[_filterbankSize];

                for (var i = 0; i < filterbank.Length; i++)
                {
                    var minPos = 0;
                    var maxPos = _fftSize / 2;

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

                    centerFrequencies[i + 1] = herzResolution * (maxPos + minPos) / 2;

                    FilterBank[i + 1] = filterbank[i];
                }

                // create arrays for duplicated edges:

                FilterBank[0] = filterbank[0];
                FilterBank[_filterbankSize - 1] = filterbank[filterbank.Length - 1];
            }

            _equalLoudnessCurve = new double[_filterbankSize];

            for (var i = 0; i < centerFrequencies.Length; i++)
            {
                var level2 = centerFrequencies[i] * centerFrequencies[i];

                _equalLoudnessCurve[i] = Math.Pow(level2 / (level2 + 1.6e5), 2) * ((level2 + 1.44e6) / (level2 + 9.61e6));
            }

            _rasta = rasta;

            if (rasta > 0)
            {
                _rastaFilters = Enumerable.Range(0, filterbankSize)
                                          .Select(f => new RastaFilter(rasta))
                                          .ToArray();
            }

            _fft = new RealFft(_fftSize);
            
            _window = window;

            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _lifterSize = lifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _preEmphasis = (float)preEmphasis;

            // buffers for LPC

            _lpcOrder = lpcOrder > 0 ? lpcOrder : FeatureCount;

            var lpcFftLength = MathUtils.NextPowerOfTwo(2 * _filterbankSize - 2);

            _lpc = new float[_lpcOrder + 1];
            _cc = new float[lpcFftLength];
            _im = new float[lpcFftLength];
            _fftLpc = new RealFft(lpcFftLength);

            // reserve memory for reusable blocks

            _spectrum = new float[_fftSize / 2 + 1];
            _bandSpectrum = new float[_filterbankSize];
            _block = new float[_fftSize];
        }

        /// <summary>
        /// Standard method for computing PLP features:
        ///     0) [Optional] pre-emphasis
        /// 
        /// Decompose signal into overlapping (hopSize) frames of length fftSize. In each frame do:
        /// 
        ///     1) Apply window (if rectangular window was specified then just do nothing)
        ///     2) Obtain power spectrum
        ///     3) Apply filterbank of critical bands
        ///     4) [Optional] filter each component of the processed spectrum with a RASTA filter
        ///     5) Apply equal loudness curve
        ///     6) Take cubic root
        ///     7) Do LPC
        ///     8) Convert LPC to cepstrum
        ///     9) [Optional] lifter the LPCC
        /// 
        /// </summary>
        /// <param name="samples">Samples for analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of PLP vectors</returns>
        public override List<FeatureVector> ComputeFrom(float[] samples, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            var hopSize = HopSize;
            var frameSize = FrameSize;

            const double power = 1.0 / 3;

            var featureVectors = new List<FeatureVector>();

            var prevSample = startSample > 0 ? samples[startSample - 1] : 0.0f;

            var lastSample = endSample - Math.Max(frameSize, hopSize);

            for (var i = startSample; i < lastSample; i += hopSize)
            {
                // prepare next block for processing

                // copy frameSize samples
                samples.FastCopyTo(_block, frameSize, i);
                // fill zeros to fftSize if frameSize < fftSize
                for (var k = frameSize; k < _block.Length; _block[k++] = 0) ;


                // 0) pre-emphasis (if needed)

                if (_preEmphasis > 1e-10)
                {
                    for (var k = 0; k < frameSize; k++)
                    {
                        var y = _block[k] - prevSample * _preEmphasis;
                        prevSample = _block[k];
                        _block[k] = y;
                    }
                    prevSample = samples[i + hopSize - 1];
                }

                // 1) apply window

                _block.ApplyWindow(_windowSamples);

                // 2) calculate power spectrum (without normalization)

                _fft.PowerSpectrum(_block, _spectrum, false);

                // 3) apply filterbank on the result (bark frequencies by default)

                FilterBanks.Apply(FilterBank, _spectrum, _bandSpectrum);

                // 4) RASTA filtering in log-domain [optional]

                if (_rasta > 0)
                {
                    for (var k = 1; k < _bandSpectrum.Length - 1; k++)
                    {
                        var log = (float)Math.Log(_bandSpectrum[k] + float.Epsilon);

                        log = _rastaFilters[k - 1].Process(log);

                        _bandSpectrum[k] = (float)Math.Exp(log);
                    }
                }

                // 5) and 6) apply equal loudness curve and take cubic root

                for (var k = 1; k < _bandSpectrum.Length - 1; k++)
                {
                    _bandSpectrum[k] = (float)Math.Pow(_bandSpectrum[k] * _equalLoudnessCurve[k], power);
                }

                // duplicate edge bands

                _bandSpectrum[0] = _bandSpectrum[1];
                _bandSpectrum[_bandSpectrum.Length - 1] = _bandSpectrum[_bandSpectrum.Length - 2];


                // 7) LPC from power spectrum:

                _bandSpectrum.FastCopyTo(_cc, _filterbankSize);

                // fill reusable buffers with zeros:

                for (var k = _filterbankSize; k < _cc.Length; _cc[k] = 0, k++) ;
                for (var k = 0; k < _im.Length; _im[k] = 0, k++) ;
                for (var k = 0; k < _lpc.Length; _lpc[k] = 0, k++) ;

                // make real part symmetric before IFFT:

                for (var k = 1; k < _filterbankSize - 1; k++)
                {
                    _cc[_filterbankSize - 1 + k] = _cc[_filterbankSize - 1 - k];
                }

                _fftLpc.Inverse(_cc, _im, _cc);

                // LPC:

                var err = MathUtils.LevinsonDurbin(_cc, _lpc, _lpcOrder);


                // 8) compute LPCC coefficients from LPC

                var lpcc = new float[FeatureCount];

                MathUtils.LpcToLpcc(_lpc, err, lpcc);
                

                // 9) (optional) liftering

                if (_lifterCoeffs != null)
                {
                    lpcc.ApplyWindow(_lifterCoeffs);
                }

                // add lpcc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = lpcc,
                    TimePosition = (double)i / SamplingRate
                });
            }

            return featureVectors;
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
                             _filterbankSize,
                             _lowFreq,
                             _highFreq,
                             _fftSize,
                              FilterBank,
                             _lifterSize,
                             _preEmphasis,
                             _window);
    }
}
