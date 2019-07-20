using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
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
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "spncc" + i).ToList();

        /// <summary>
        /// Forgetting factor in formula (15) in [Kim & Stern, 2016]
        /// </summary>
        public float LambdaMu { get; set; } = 0.999f;

        /// <summary>
        /// Filterbank (gammatone by default)
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// Lower frequency
        /// </summary>
        protected readonly double _lowFreq;

        /// <summary>
        /// Upper frequency
        /// </summary>
        protected readonly double _highFreq;

        /// <summary>
        /// Nonlinearity coefficient (if 0 then Log10 is applied)
        /// </summary>
        protected readonly int _power;

        /// <summary>
        /// FFT transformer
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// DCT-II transformer
        /// </summary>
        protected readonly Dct2 _dct;

        /// <summary>
        /// Type of the window function
        /// </summary>
        protected readonly WindowTypes _window;

        /// <summary>
        /// Window samples
        /// </summary>
        protected readonly float[] _windowSamples;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for gammatone spectrum
        /// </summary>
        protected readonly float[] _filteredSpectrum;

        /// <summary>
        /// Value for mean normalization
        /// </summary>
        protected float _mean = 4e07f;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="frameDuration">Length of analysis window (in seconds)</param>
        /// <param name="hopDuration">Length of overlap (in seconds)</param>
        /// <param name="power"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="filterbank"></param>
        /// <param name="fftSize">Size of FFT (in samples)</param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public SpnccExtractor(int samplingRate, 
                              int featureCount,
                              double frameDuration = 0.0256/*sec*/,
                              double hopDuration = 0.010/*sec*/,
                              int power = 15,
                              double lowFreq = 100,
                              double highFreq = 6800,
                              int filterbankSize = 40,
                              float[][] filterbank = null,
                              int fftSize = 0,
                              double preEmphasis = 0,
                              WindowTypes window = WindowTypes.Hamming)

            : base(samplingRate, frameDuration, hopDuration, preEmphasis)
        {
            FeatureCount = featureCount;

            _power = power;

            if (filterbank == null)
            {
                _blockSize = fftSize > FrameSize ? fftSize : MathUtils.NextPowerOfTwo(FrameSize);

                _lowFreq = lowFreq;
                _highFreq = highFreq;

                FilterBank = FilterBanks.Erb(filterbankSize, _blockSize, samplingRate, _lowFreq, _highFreq);
            }
            else
            {
                FilterBank = filterbank;
                filterbankSize = filterbank.Length;
                _blockSize = 2 * (filterbank[0].Length - 1);

                Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");
            }

            _fft = new RealFft(_blockSize);
            _dct = new Dct2(filterbankSize);
            
            _window = window;
            _windowSamples = Window.OfType(_window, FrameSize);

            _spectrum = new float[_blockSize / 2 + 1];
            _filteredSpectrum = new float[filterbankSize];
        }

        /// <summary>
        /// S(implified)PNCC algorithm according to [Kim & Stern, 2016].
        /// In each frame do:
        /// 
        ///     1) Apply window (if rectangular window was specified then just do nothing)
        ///     2) Obtain power spectrum
        ///     3) Apply gammatone filters (squared)
        ///     4) Mean power normalization
        ///     5) Apply nonlinearity
        ///     6) Do dct-II (normalized)
        /// 
        /// </summary>
        /// <param name="samples">Samples for analysis</param>
        /// <returns>List of pncc vectors</returns>
        public override float[] ProcessFrame(float[] block)
        {
            const float meanPower = 1e10f;

            // fill zeros to fftSize if frameSize < fftSize

            for (var k = FrameSize; k < block.Length; block[k++] = 0) ;

            // 1) apply window

            block.ApplyWindow(_windowSamples);

            // 2) calculate power spectrum

            _fft.PowerSpectrum(block, _spectrum, false);

            // 3) apply gammatone filterbank

            FilterBanks.Apply(FilterBank, _spectrum, _filteredSpectrum);

            // 4) mean power normalization:

            var sumPower = 0.0f;
            for (var j = 0; j < _filteredSpectrum.Length; j++)
            {
                sumPower += _filteredSpectrum[j];
            }

            _mean = LambdaMu * _mean + (1 - LambdaMu) * sumPower;

            for (var j = 0; j < _filteredSpectrum.Length; j++)
            {
                _filteredSpectrum[j] *= meanPower / _mean;
            }

            // 5) nonlinearity (pow ^ d  or  Log10)

            if (_power != 0)
            {
                for (var j = 0; j < _filteredSpectrum.Length; j++)
                {
                    _filteredSpectrum[j] = (float)Math.Pow(_filteredSpectrum[j], 1.0 / _power);
                }
            }
            else
            {
                for (var j = 0; j < _filteredSpectrum.Length; j++)
                {
                    _filteredSpectrum[j] = (float)Math.Log10(_filteredSpectrum[j] + float.Epsilon);
                }
            }

            // 6) dct-II (normalized)

            var spnccs = new float[FeatureCount];
            _dct.DirectNorm(_filteredSpectrum, spnccs);

            return spnccs;
        }

        /// <summary>
        /// Reset extractor
        /// </summary>
        public override void Reset()
        {
            _mean = 4e07f;
        }
    }
}
