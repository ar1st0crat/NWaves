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
        public override IEnumerable<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "spncc" + i);

        /// <summary>
        /// Forgetting factor in formula (15) in [Kim & Stern, 2016]
        /// </summary>
        public double LambdaMu { get; set; } = 0.999;

        /// <summary>
        /// Gammatone Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        public double[][] GammatoneFilterBank { get; }

        /// <summary>
        /// Nonlinearity coefficient (if 0 then Log10 is applied)
        /// </summary>
        private readonly int _power;

        /// <summary>
        /// Size of FFT (in samples)
        /// </summary>
        private readonly int _fftSize;

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

            GammatoneFilterBank = FilterBanks.Erb(filterbankSize, _fftSize, samplingRate, lowFreq, highFreq);
            
            // use power spectrum
            foreach (var filter in GammatoneFilterBank)
            {
                for (var j = 0; j < filter.Length; j++)
                {
                    var ps = filter[j] * filter[j];
                    filter[j] = ps;
                }
            }
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
        /// <returns>List of pncc vectors</returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            var gammatoneSpectrum = new double[GammatoneFilterBank.Length];
            
            const double meanPower = 1e10;
            var mean = 4e07;

            var d = _power != 0 ? 1.0 / _power : 0.0;

            var block = new double[_fftSize];
            var zeroblock = new double[_fftSize - _windowSamples.Length];
            
            // prepare everything for dct

            var dct = new Dct();
            dct.Init(GammatoneFilterBank.Length, FeatureCount);


            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;


            var timePos = 0;
            while (timePos + _windowSamples.Length < filtered.Samples.Length)
            {
                // prepare next block for processing

                FastCopy.ToExistingArray(filtered.Samples, block, _windowSamples.Length, timePos);
                FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length, 0, _windowSamples.Length);


                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }


                // 2) calculate power spectrum

                var spectrum = Transform.PowerSpectrum(block, _fftSize);


                // 3) apply gammatone filterbank

                ApplyFilterbank(spectrum, gammatoneSpectrum);


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
                dct.Dct2N(gammatoneSpectrum, spnccs);


                // add pncc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = spnccs,
                    TimePosition = (double)timePos / signal.SamplingRate
                });

                timePos += _hopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// Method applies gammatone filters to spectrum.
        /// </summary>
        /// <param name="spectrum">Original spectrum</param>
        /// <param name="gammatoneSpectrum">Output gammatone-spectral array</param>
        private void ApplyFilterbank(double[] spectrum, double[] gammatoneSpectrum)
        {
            for (var i = 0; i < GammatoneFilterBank.Length; i++)
            {
                gammatoneSpectrum[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    gammatoneSpectrum[i] += GammatoneFilterBank[i][j] * spectrum[j];
                }
            }
        }
    }
}
