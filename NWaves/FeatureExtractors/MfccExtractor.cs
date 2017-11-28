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
        public override string[] FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "mfcc" + i).ToArray();

        /// <summary>
        /// Mel Filterbank matrix of dimension [melFilterCount * (fftSize/2 + 1)]
        /// </summary>
        private readonly double[][] _melFilterBank;
        public double[][] MelFilterBank => _melFilterBank;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Internal DCT transformer
        /// </summary>
        private readonly Dct _dct;

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

            _melFilterBank = FilterBanks.Mel(melFilterbankSize, _fftSize, samplingRate, lowFreq, highFreq);

            // prepare everything for fft and dct

            _fft = new Fft(_fftSize);
            _dct = new Dct(_melFilterBank.Length, featureCount);
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
            var featureVectors = new List<FeatureVector>();
            
            // reserve memory for reusable blocks

            var spectrum = new double[_fftSize / 2 + 1];
            var logMelSpectrum = new double[_melFilterBank.Length];

            var block = new double[_fftSize];       // buffer for currently processed signal block at each step
            var zeroblock = new double[_fftSize];   // just a buffer of zeros for quick memset


            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;
            

            var i = startSample;
            while (i + _windowSamples.Length < endSample)
            {
                // prepare next block for processing

                FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length);
                FastCopy.ToExistingArray(filtered.Samples, block, _windowSamples.Length, i);
                

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }


                // 2) calculate power spectrum

                _fft.PowerSpectrum(block, spectrum);


                // 3) apply mel filterbank and take log() of the result

                FilterBanks.ApplyAndLog(_melFilterBank, spectrum, logMelSpectrum);


                // 4) dct-II

                var mfccs = new double[FeatureCount];
                _dct.Dct2(logMelSpectrum, mfccs);


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
    }
}
