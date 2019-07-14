using NWaves.Filters.Fda;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// MFCC FB-24 extractor (used in HTK, uniform mel bands)
    /// </summary>
    public class MfccExtractorHtk : MfccExtractor
    {
        public MfccExtractorHtk(int samplingRate,
                                int featureCount,
                                double frameDuration = 0.0256/*sec*/,
                                double hopDuration = 0.010/*sec*/,
                                int filterbankSize = 24,
                                double lowFreq = 0,
                                double highFreq = 0,
                                int fftSize = 0,
                                int lifterSize = 0,
                                double preEmphasis = 0,
                                bool includeEnergy = false,
                                SpectrumType spectrumType = SpectrumType.Power,
                                WindowTypes window = WindowTypes.Hamming)
            : base(samplingRate,
                   featureCount,
                   frameDuration,
                   hopDuration,
                   filterbankSize,
                   lowFreq,
                   highFreq,
                   fftSize,
                   MakeFilterbank(filterbankSize,
                                  samplingRate,
                                  fftSize,
                                  frameDuration,
                                  lowFreq,
                                  highFreq),
                   lifterSize,
                   preEmphasis,
                   includeEnergy,
                   "2N",
                   NonLinearityType.LogE,
                   spectrumType,
                   window,
                   1.0f)
        {
        }

        private static float[][] MakeFilterbank(int filterbankSize,
                                                int samplingRate,
                                                int fftSize,
                                                double frameDuration,
                                                double lowFreq = 0,
                                                double highFreq = 0)
        {
            var frameSize = (int)(frameDuration * samplingRate);

            fftSize = fftSize > frameSize ? fftSize : MathUtils.NextPowerOfTwo(frameSize);

            var melBands = FilterBanks.MelBands(filterbankSize, fftSize, samplingRate, lowFreq, highFreq);

            return FilterBanks.Triangular(fftSize, samplingRate, melBands, null, Scale.HerzToMel);
        }
    }
}
