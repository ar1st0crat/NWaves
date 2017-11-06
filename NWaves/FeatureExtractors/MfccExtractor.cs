using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Signals;
using NWaves.Transforms;
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
        /// Mel Filterbanks matrix of dimension [melFilterbanks * (fftSize/2 + 1)]
        /// </summary>
        public double[][] MelFilterBanks { get; }

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
        /// <param name="melFilterbanks"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MfccExtractor(int featureCount, int samplingRate,
                             int melFilterbanks = 20, double lowFreq = 0, double highFreq = 0,
                             int fftSize = 512, int hopSize = 256, int lifterSize = 22,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;
            _fftSize = fftSize;
            _hopSize = hopSize;
            _window = window;
            _windowSamples = Window.OfType(window, fftSize);
            _lifterCoeffs = Window.Liftering(featureCount, lifterSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            MelFilterBanks = FilterBanks.Mel(melFilterbanks, fftSize, samplingRate, lowFreq, highFreq);
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
            dct.Init(MelFilterBanks.Length, FeatureCount);


            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;
            
            
            var logMelSpectrum = new double[MelFilterBanks.Length];

            var i = 0;
            while (i + _fftSize < filtered.Samples.Length)
            {
                var x = filtered[i, i + _fftSize].Samples;
                

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    x.ApplyWindow(_windowSamples);
                }


                // 2) calculate power spectrum

                var spectrum = Transform.PowerSpectrum(x, _fftSize);


                // 3) apply mel filterbanks and take log() of the result

                ApplyMelFilterbankLog(spectrum, logMelSpectrum);


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
        private void ApplyMelFilterbankLog(double[] spectrum, double[] logMelSpectrum)
        {
            for (var i = 0; i < MelFilterBanks.Length; i++)
            {
                logMelSpectrum[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    logMelSpectrum[i] += MelFilterBanks[i][j] * spectrum[j];
                }

                logMelSpectrum[i] = Math.Log10(logMelSpectrum[i] + double.Epsilon);
            }
        }
    }
}
