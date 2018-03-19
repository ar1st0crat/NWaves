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
        public float LambdaMu { get; set; } = 0.999f;

        /// <summary>
        /// Gammatone Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)]
        /// </summary>
        private float[][] _gammatoneFilterBank;
        public float[][] FilterBank => _gammatoneFilterBank;

        /// <summary>
        /// Number of gammatone filters
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
        /// Nonlinearity coefficient (if 0 then Log10 is applied)
        /// </summary>
        private readonly int _power;

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
        /// <param name="power"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="windowSize">Length of analysis window (in seconds)</param>
        /// <param name="hopSize">Length of overlap (in seconds)</param>
        /// <param name="fftSize">Size of FFT (in samples)</param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public SpnccExtractor(int featureCount, int power = 15,
                             int filterbankSize = 40, float lowFreq = 100, float highFreq = 6800,
                             double windowSize = 0.0256/*sec*/, double hopSize = 0.010/*sec*/, int fftSize = 1024,
                             float preEmphasis = 0.0f, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;
            _power = power;

            _window = window;
            _windowSize = windowSize;
            _hopSize = hopSize;
            _fftSize = fftSize;

            _filterbankSize = filterbankSize;
            _lowFreq = lowFreq;
            _highFreq = highFreq;

            _preEmphasis = preEmphasis;
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
        ///     4) Mean power normalization
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
            // ====================================== PREPARE =======================================

            var hopSize = (int)(signal.SamplingRate * _hopSize);
            var windowSize = (int)(signal.SamplingRate * _windowSize);
            var windowSamples = Window.OfType(_window, windowSize);

            var fftSize = _fftSize >= windowSize ? _fftSize : MathUtils.NextPowerOfTwo(windowSize);

            _gammatoneFilterBank = FilterBanks.Erb(_filterbankSize, _fftSize, signal.SamplingRate, _lowFreq, _highFreq);

            // use power spectrum:

            foreach (var filter in _gammatoneFilterBank)
            {
                for (var j = 0; j < filter.Length; j++)
                {
                    var ps = filter[j] * filter[j];
                    filter[j] = ps;
                }
            }


            var fft = new Fft (fftSize);
            var dct = new Dct2(_filterbankSize, FeatureCount);


            var gammatoneSpectrum = new float[_filterbankSize];
            
            const float meanPower = 1e10f;
            var mean = 4e07f;

            var d = _power != 0 ? 1.0 / _power : 0.0;

            var block = new float[fftSize];           // buffer for a signal block at each step
            var zeroblock = new float[fftSize];       // buffer of zeros for quick memset

            var spectrum = new float[fftSize / 2 + 1];


            // 0) pre-emphasis (if needed)

            if (_preEmphasis > 0.0)
            {
                var preemphasisFilter = new PreEmphasisFilter(_preEmphasis);
                signal = preemphasisFilter.ApplyTo(signal);
            }


            // ================================= MAIN PROCESSING ==================================

            var featureVectors = new List<FeatureVector>();

            var i = startSample;
            while (i + windowSize < endSample)
            {
                // prepare next block for processing

                zeroblock.FastCopyTo(block, zeroblock.Length);
                signal.Samples.FastCopyTo(block, windowSize, i);
                

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(windowSamples);
                }


                // 2) calculate power spectrum

                fft.PowerSpectrum(block, spectrum);


                // 3) apply gammatone filterbank

                FilterBanks.Apply(_gammatoneFilterBank, spectrum, gammatoneSpectrum);


                // 4) mean power normalization:

                var sumPower = 0.0f;
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
                        gammatoneSpectrum[j] = (float)Math.Pow(gammatoneSpectrum[j], d);
                    }
                }
                else
                {
                    for (var j = 0; j < gammatoneSpectrum.Length; j++)
                    {
                        gammatoneSpectrum[j] = (float)Math.Log10(gammatoneSpectrum[j] + float.Epsilon);
                    }
                }


                // 6) dct-II (normalized)

                var spnccs = new float[FeatureCount];
                dct.DirectN(gammatoneSpectrum, spnccs);


                // add pncc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = spnccs,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += hopSize;
            }

            return featureVectors;
        }
    }
}
