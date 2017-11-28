using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Simplified Power-Normalized Cepstral Coefficients extractor
    /// </summary>
    public class SpnccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (including coeff #0)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "spncc0", "spncc1", "spncc2", etc.)
        /// </summary>
        public override string[] FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "spncc" + i).ToArray();

        /// <summary>
        /// Forgetting factor in formula (15) in [Kim & Stern, 2016]
        /// </summary>
        public double LambdaMu { get; set; } = 0.999;

        /// <summary>
        /// Gammatone Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        private readonly double[][] _gammatoneFilterBank;
        public double[][] GammatoneFilterBank => _gammatoneFilterBank;

        /// <summary>
        /// Nonlinearity coefficient (if 0 then Log10 is applied)
        /// </summary>
        private readonly int _power;

        /// <summary>
        /// Size of FFT (in samples)
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
        /// Size of overlap (in samples)
        /// </summary>
        private readonly int _hopSize;

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
        /// <param name="power"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="windowSize">Length of analysis window (in seconds)</param>
        /// <param name="overlapSize">Length of overlap (in seconds)</param>
        /// <param name="fftSize">Size of FFT (in samples)</param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public SpnccExtractor(int featureCount, int samplingRate, int power = 15,
                             int filterbankSize = 40, double lowFreq = 100, double highFreq = 6800,
                             double windowSize = 0.0256, double overlapSize = 0.010, int fftSize = 1024,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;
            _power = power;

            var windowLength = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, windowLength);
            _window = window;

            _fftSize = fftSize >= windowLength ? fftSize : MathUtils.NextPowerOfTwo(windowLength);
            _hopSize = (int)(samplingRate * overlapSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            _gammatoneFilterBank = FilterBanks.Erb(filterbankSize, _fftSize, samplingRate, lowFreq, highFreq);
            
            // use power spectrum
            foreach (var filter in _gammatoneFilterBank)
            {
                for (var j = 0; j < filter.Length; j++)
                {
                    var ps = filter[j] * filter[j];
                    filter[j] = ps;
                }
            }

            // prepare everything for fft and dct

            _fft = new Fft(_fftSize);
            _dct = new Dct(_gammatoneFilterBank.Length, featureCount);
        }

        /// <summary>
        /// S(implified)PNCC algorithm according to [Kim & Stern, 2016]:
        ///     0) [Optional] pre-emphasis
        /// 
        /// Decompose signal into overlapping (hopSize) frames of length fftSize. In each frame do:
        /// 
        ///     1) Apply window (if rectangular window was specified then just do nothing)
        ///     2) Obtain power spectrum
        ///     3) Apply gammatone filters (squared)
        ///     4) Medium-time processing (asymmetric noise suppression, temporal masking, spectral smoothing)
        ///     5) Apply nonlinearity
        ///     6) Do dct-II (normalized)
        /// 
        /// </summary>
        /// <param name="signal">Signal for analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of pncc vectors</returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            var featureVectors = new List<FeatureVector>();

            var gammatoneSpectrum = new double[_gammatoneFilterBank.Length];
            
            const double meanPower = 1e10;
            var mean = 4e07;

            var d = _power != 0 ? 1.0 / _power : 0.0;

            var block = new double[_fftSize];           // buffer for a signal block at each step
            var zeroblock = new double[_fftSize];       // buffer of zeros for quick memset

            var spectrum = new double[_fftSize / 2 + 1];


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


                // 3) apply gammatone filterbank

                FilterBanks.Apply(_gammatoneFilterBank, spectrum, gammatoneSpectrum);


                // 4) mean power normalization:

                var sumPower = 0.0;
                for (var j = 0; j < gammatoneSpectrum.Length; j++)
                {
                    sumPower += gammatoneSpectrum[j];
                }

                mean = LambdaMu * mean + (1 - LambdaMu) * sumPower;

                for (var j = 0; j < gammatoneSpectrum.Length; j++)
                {
                    gammatoneSpectrum[j] *= meanPower / mean;
                }
                

                // 5) nonlinearity (power ^ d     or     Log10)

                if (_power != 0)
                {
                    for (var j = 0; j < gammatoneSpectrum.Length; j++)
                    {
                        gammatoneSpectrum[j] = Math.Pow(gammatoneSpectrum[j], d);
                    }
                }
                else
                {
                    for (var j = 0; j < gammatoneSpectrum.Length; j++)
                    {
                        gammatoneSpectrum[j] = Math.Log10(gammatoneSpectrum[j] + double.Epsilon);
                    }
                }


                // 6) dct-II (normalized)

                var spnccs = new double[FeatureCount];
                _dct.Dct2N(gammatoneSpectrum, spnccs);


                // add pncc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = spnccs,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _hopSize;
            }

            return featureVectors;
        }
    }
}
