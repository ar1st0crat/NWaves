using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Simplified Power-Normalized Cepstral Coefficients extractor
    /// </summary>
    public class SpnccExtractor : FeatureExtractor
    {
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
        /// <param name="options">PNCC options</param>
        public SpnccExtractor(PnccOptions options) : base(options)
        {
            FeatureCount = options.FeatureCount;

            var filterbankSize = options.FilterBankSize;

            if (options.FilterBank == null)
            {
                _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);

                _lowFreq = options.LowFrequency;
                _highFreq = options.HighFrequency;

                FilterBank = FilterBanks.Erb(filterbankSize, _blockSize, SamplingRate, _lowFreq, _highFreq);
            }
            else
            {
                FilterBank = options.FilterBank;
                filterbankSize = FilterBank.Length;
                _blockSize = 2 * (FilterBank[0].Length - 1);

                Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");
            }

            _power = options.Power;

            _fft = new RealFft(_blockSize);
            _dct = new Dct2(filterbankSize);

            _spectrum = new float[_blockSize / 2 + 1];
            _filteredSpectrum = new float[filterbankSize];
        }

        /// <summary>
        /// S(implified)PNCC algorithm according to [Kim & Stern, 2016].
        /// In each frame do:
        /// 
        ///     0) Apply window (base extractor does it)
        ///     1) Obtain power spectrum
        ///     2) Apply gammatone filters (squared)
        ///     3) Mean power normalization
        ///     4) Apply nonlinearity
        ///     5) Do dct-II (normalized)
        /// 
        /// </summary>
        /// <param name="block">Block of samples for analysis</param>
        /// <param name="features">List of spncc vectors</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            const float meanPower = 1e10f;

            // 1) calculate power spectrum

            _fft.PowerSpectrum(block, _spectrum, false);

            // 2) apply gammatone filterbank

            FilterBanks.Apply(FilterBank, _spectrum, _filteredSpectrum);

            // 3) mean power normalization:

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

            // 4) nonlinearity (pow ^ d  or  Log10)

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

            // 5) dct-II (normalized)

            _dct.DirectNorm(_filteredSpectrum, features);
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
