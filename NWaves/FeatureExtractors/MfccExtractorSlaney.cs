using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Fda;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// MFCC FB-40 extractor (used in Slaney's Auditory Toolbox)
    /// </summary>
    public class MfccExtractorSlaney : MfccExtractor
    {
        protected bool _normalizeFilterbank = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="fftSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="includeEnergy"></param>
        /// <param name="spectrumType"></param>
        /// <param name="window"></param>
        /// <param name="logFloor"></param>
        /// <param name="normalizeFilterbank"></param>
        public MfccExtractorSlaney(int samplingRate,
                                   int featureCount,
                                   double frameDuration = 0.0256/*sec*/,
                                   double hopDuration = 0.010/*sec*/,
                                   int filterbankSize = 40,
                                   double lowFreq = 0,
                                   double highFreq = 0,
                                   int fftSize = 0,
                                   int lifterSize = 0,
                                   double preEmphasis = 0,
                                   bool includeEnergy = false,
                                   SpectrumType spectrumType = SpectrumType.Power,
                                   WindowTypes window = WindowTypes.Hamming,
                                   float logFloor = float.Epsilon,
                                   bool normalizeFilterbank = true)
            : base(samplingRate,
                   featureCount,
                   frameDuration,
                   hopDuration,
                   filterbankSize,
                   lowFreq,
                   highFreq,
                   fftSize,
                   MakeFilterbank(filterbankSize,       // Slaney's filter bank
                                  samplingRate,
                                  fftSize,
                                  frameDuration,
                                  lowFreq,
                                  highFreq,
                                  normalizeFilterbank),
                   lifterSize,
                   preEmphasis,
                   includeEnergy,
                   "2N",
                   NonLinearityType.LogE,
                   spectrumType,
                   window,
                   logFloor)
        {
            _normalizeFilterbank = normalizeFilterbank;
        }

        /// <summary>
        /// Make Slaney's filter bank
        /// </summary>
        /// <param name="filterbankSize"></param>
        /// <param name="samplingRate"></param>
        /// <param name="fftSize"></param>
        /// <param name="frameDuration"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        private static float[][] MakeFilterbank(int filterbankSize,
                                                int samplingRate,
                                                int fftSize,
                                                double frameDuration,
                                                double lowFreq = 0,
                                                double highFreq = 0,
                                                bool normalize = true)
        {
            var frameSize = (int)(frameDuration * samplingRate);

            fftSize = fftSize > frameSize ? fftSize : MathUtils.NextPowerOfTwo(frameSize);

            return FilterBanks.MelBankSlaney(filterbankSize, fftSize, samplingRate, lowFreq, highFreq, normalize);
        }

        /// <summary>
        /// True if computations can be done in parallel
        /// </summary>
        /// <returns></returns>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() =>
            new MfccExtractorSlaney( SamplingRate,
                                     FeatureCount,
                                     FrameDuration,
                                     HopDuration,
                                     FilterBank.Length,
                                    _lowFreq,
                                    _highFreq,
                                    _blockSize,
                                    _lifterSize,
                                    _preEmphasis,
                                    _includeEnergy,
                                    _spectrumType,
                                    _window,
                                    _logFloor,
                                    _normalizeFilterbank);
    }
}
