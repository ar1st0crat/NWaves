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
    /// Simplified Power-Normalized Cepstral Coefficients (SPNCC) extractor.
    /// </summary>
    public class SpnccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Feature names (simply "spncc0", "spncc1", "spncc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions
        {
            get
            {
                var names = Enumerable.Range(0, FeatureCount).Select(i => "spncc" + i).ToList();
                if (_includeEnergy) names[0] = "log_En";
                return names;
            }
        }

        /// <summary>
        /// Forgetting factor in formula (15) in [Kim, Stern, 2016].
        /// </summary>
        public float LambdaMu { get; set; } = 0.999f;

        /// <summary>
        /// Filterbank (gammatone by default).
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// Nonlinearity coefficient (if 0 then Log10 is applied).
        /// </summary>
        protected readonly int _power;

        /// <summary>
        /// Should the first SPNCC coefficient be replaced with LOG(energy).
        /// </summary>
        protected readonly bool _includeEnergy;

        /// <summary>
        /// Floor value for LOG-energy calculation.
        /// </summary>
        protected readonly float _logEnergyFloor;

        /// <summary>
        /// FFT transformer.
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// DCT-II transformer.
        /// </summary>
        protected readonly Dct2 _dct;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step.
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for gammatone spectrum.
        /// </summary>
        protected readonly float[] _filteredSpectrum;

        /// <summary>
        /// Value for mean normalization.
        /// </summary>
        protected float _mean = 4e07f;

        /// <summary>
        /// Construct extractor from configuration options.
        /// </summary>
        /// <param name="options">Extractor configuration options</param>
        public SpnccExtractor(PnccOptions options) : base(options)
        {
            FeatureCount = options.FeatureCount;

            var filterbankSize = options.FilterBankSize;

            if (options.FilterBank is null)
            {
                _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);

                FilterBank = FilterBanks.Erb(filterbankSize, _blockSize, SamplingRate, options.LowFrequency, options.HighFrequency);
            }
            else
            {
                FilterBank = options.FilterBank;
                filterbankSize = FilterBank.Length;
                _blockSize = 2 * (FilterBank[0].Length - 1);

                Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");
            }

            _power = options.Power;

            _includeEnergy = options.IncludeEnergy;
            _logEnergyFloor = options.LogEnergyFloor;

            _fft = new RealFft(_blockSize);
            _dct = new Dct2(filterbankSize);

            _spectrum = new float[_blockSize / 2 + 1];
            _filteredSpectrum = new float[filterbankSize];
        }

        /// <summary>
        /// <para>Compute S(implified)PNCC vector in one frame according to [Kim and Stern, 2016].</para>
        /// <para>
        /// General algorithm:
        /// <list type="number">
        ///     <item>Apply window</item>
        ///     <item>Obtain power spectrum</item>
        ///     <item>Apply gammatone filters (squared)</item>
        ///     <item>Mean power normalization</item>
        ///     <item>Apply nonlinearity</item>
        ///     <item>Do DCT-II (normalized)</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one SPNCC feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            const float meanPower = 1e10f;

            // 0) base extractor applies window

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

            // 6) (optional) replace first coeff with log(energy) 

            if (_includeEnergy)
            {
                features[0] = (float)Math.Log(Math.Max(block.Sum(x => x * x), _logEnergyFloor));
            }
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
