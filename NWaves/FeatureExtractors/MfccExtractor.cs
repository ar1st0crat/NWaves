using System;
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
        public override IEnumerable<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "mfcc" + i);

        /// <summary>
        /// Mel Filterbank matrix of dimension [melFilterCount * (fftSize/2 + 1)]
        /// </summary>
        public double[][] MelFilterBank { get; }

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Size of overlap
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Coefficients of the liftering window
        /// </summary>
        private readonly double[] _lifterCoeffs;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Samples of the weighting window
        /// </summary>
        private readonly double[] _windowSamples;

        /// <summary>
        /// Pre-emphasis filter (if needed)
        /// </summary>
        private readonly PreEmphasisFilter _preemphasisFilter;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="samplingRate"></param>
        /// <param name="melFilterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="windowSize"></param>
        /// <param name="overlapSize"></param>
        /// <param name="fftSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MfccExtractor(int featureCount, int samplingRate,
                             int melFilterbankSize = 20, double lowFreq = 0, double highFreq = 0,
                             double windowSize = 0.0256, double overlapSize = 0.010, int fftSize = 0, int lifterSize = 22,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;

            var windowLength = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, windowLength);
            _window = window;

            _fftSize = fftSize >= windowLength ? fftSize : MathUtils.NextPowerOfTwo(windowLength);
            _hopSize = (int)(samplingRate * overlapSize);
            
            _lifterCoeffs = Window.Liftering(featureCount, lifterSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            MelFilterBank = FilterBanks.Mel(melFilterbankSize, _fftSize, samplingRate, lowFreq, highFreq);
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
        /// <returns>List of mfcc vectors</returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            // prepare everything for dct

            var dct = new Dct();
            dct.Init(MelFilterBank.Length, FeatureCount);


            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;
            
            
            var logMelSpectrum = new double[MelFilterBank.Length];

            var block = new double[_fftSize];
            var zeroblock = new double[_fftSize - _windowSamples.Length];

            var i = 0;
            while (i + _windowSamples.Length < filtered.Length)
            {
                // prepare next block for processing

                FastCopy.ToExistingArray(filtered.Samples, block, _windowSamples.Length, i);
                FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length, 0, _windowSamples.Length);


                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }


                // 2) calculate power spectrum

                var spectrum = Transform.PowerSpectrum(block, _fftSize);


                // 3) apply mel filterbank and take log() of the result

                ApplyFilterbankAndLog(spectrum, logMelSpectrum);


                // 4) dct-II

                var mfccs = new double[FeatureCount];
                dct.Dct2(logMelSpectrum, mfccs);


                // 5) (optional) liftering

                mfccs.ApplyWindow(_lifterCoeffs);


                // add mfcc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = mfccs,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _hopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// Method applies mel filters to spectrum and then does Log10() on resulting spectrum.
        /// </summary>
        /// <param name="spectrum">Original spectrum</param>
        /// <param name="logMelSpectrum">Output log-mel-spectral array</param>
        private void ApplyFilterbankAndLog(double[] spectrum, double[] logMelSpectrum)
        {
            for (var i = 0; i < MelFilterBank.Length; i++)
            {
                logMelSpectrum[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    logMelSpectrum[i] += MelFilterBank[i][j] * spectrum[j];
                }

                logMelSpectrum[i] = Math.Log10(logMelSpectrum[i] + double.Epsilon);
            }
        }
    }
}
