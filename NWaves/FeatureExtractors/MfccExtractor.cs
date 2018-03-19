using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Mel Frequency Cepstral Coefficients extractor
    /// </summary>
    public class MfccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (including coeff #0)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "mfcc0", "mfcc1", "mfcc2", etc.)
        /// </summary>
        public override string[] FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "mfcc" + i).ToArray();

        /// <summary>
        /// Mel Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)]
        /// </summary>
        private float[][] _melFilterBank;
        public float[][] FilterBank => _melFilterBank;

        /// <summary>
        /// Number of mel filters
        /// </summary>
        private readonly int _filterbankSize;

        /// <summary>
        /// Lower frequency
        /// </summary>
        private readonly float _lowFreq;

        /// <summary>
        /// Upper frequency
        /// </summary>
        private readonly float _highFreq;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Length of analysis window (in seconds)
        /// </summary>
        private readonly double _windowSize;

        /// <summary>
        /// Hop length (in seconds)
        /// </summary>
        private readonly double _hopSize;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        private readonly int _lifterSize;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly float _preEmphasis;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="melFilterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MfccExtractor(int featureCount,
                             int melFilterbankSize = 20, float lowFreq = 0, float highFreq = 0,
                             double windowSize = 0.0256/*sec*/, double hopSize = 0.010/*sec*/,
                             int fftSize = 0, int lifterSize = 22,
                             float preEmphasis = 0.0f, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;

            _window = window;
            _windowSize = windowSize;
            _hopSize = hopSize;
            _fftSize = fftSize;

            _filterbankSize = melFilterbankSize;
            _lowFreq = lowFreq;
            _highFreq = highFreq;

            _lifterSize = lifterSize;
            _preEmphasis = preEmphasis;
        }

        /// <summary>
        /// Standard method for computing mfcc features:
        ///     0) [Optional] pre-emphasis
        /// 
        /// Decompose signal into overlapping (hopSize) frames of length fftSize. In each frame do:
        /// 
        ///     1) Apply window (if rectangular window was specified then just do nothing)
        ///     2) Obtain power spectrum X
        ///     3) Apply mel filters and log() the result: Y = Log10(X * H)
        ///     4) Do dct-II: mfcc = Dct(Y)
        ///     5) [Optional] liftering of mfcc
        /// 
        /// </summary>
        /// <param name="signal">Signal for analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of mfcc vectors</returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            // ====================================== PREPARE =======================================

            var hopSize = (int)(signal.SamplingRate * _hopSize);
            var windowSize = (int)(signal.SamplingRate * _windowSize);
            var windowSamples = Window.OfType(_window, windowSize);
            
            var fftSize = _fftSize >= windowSize ? _fftSize : MathUtils.NextPowerOfTwo(windowSize);
            
            _melFilterBank = FilterBanks.Triangular(fftSize, signal.SamplingRate,
                                FilterBanks.MelBands(_filterbankSize, fftSize, signal.SamplingRate, _lowFreq, _highFreq));

            var lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            var fft = new Fft (fftSize);
            var dct = new Dct2(_filterbankSize, FeatureCount);


            // reserve memory for reusable blocks

            var spectrum = new float[fftSize / 2 + 1];
            var logMelSpectrum = new float[_filterbankSize];

            var block = new float[fftSize];       // buffer for currently processed signal block at each step
            var zeroblock = new float[fftSize];   // just a buffer of zeros for quick memset


            // 0) pre-emphasis (if needed)

            if (_preEmphasis > 0.0)
            {
                var preemphasisFilter = new PreEmphasisFilter(_preEmphasis);
                signal = preemphasisFilter.ApplyTo(signal);
            }
            

            // ================================= MAIN PROCESSING ==================================

            var featureVectors = new List<FeatureVector>();

            var i = startSample;
            while (i + windowSamples.Length < endSample)
            {
                // prepare next block for processing

                FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length);
                FastCopy.ToExistingArray(signal.Samples, block, windowSamples.Length, i);
                

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(windowSamples);
                }


                // 2) calculate power spectrum

                fft.PowerSpectrum(block, spectrum);


                // 3) apply mel filterbank and take log() of the result

                FilterBanks.ApplyAndLog(_melFilterBank, spectrum, logMelSpectrum);


                // 4) dct-II

                var mfccs = new float[FeatureCount];
                dct.Direct(logMelSpectrum, mfccs);


                // 5) (optional) liftering

                if (lifterCoeffs != null)
                {
                    mfccs.ApplyWindow(lifterCoeffs);
                }


                // add mfcc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = mfccs,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += hopSize;
            }

            return featureVectors;
        }
    }
}
