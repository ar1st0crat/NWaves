using NWaves.Filters.Fda;
using NWaves.Utils;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class MfccHtkOptions : MfccOptions
    {
        public MfccHtkOptions(int samplingRate,
                              int featureCount,
                              double frameDuration,
                              double lowFrequency = 0,
                              double highFrequency = 0,
                              int filterbankSize = 24,
                              int fftSize = 0)
        {
            
            var frameSize = (int)(frameDuration * samplingRate);
            fftSize = fftSize > frameSize ? fftSize : MathUtils.NextPowerOfTwo(frameSize);

            var melBands = FilterBanks.MelBands(filterbankSize, samplingRate, lowFrequency, highFrequency);
            FilterBank = FilterBanks.Triangular(fftSize, samplingRate, melBands, null, Scale.HerzToMel);
            FilterBankSize = filterbankSize;
            FeatureCount = featureCount;
            FftSize = fftSize;
            SamplingRate = samplingRate;
            LowFrequency = lowFrequency;
            HighFrequency = highFrequency;
            NonLinearity = NonLinearityType.LogE;
            LogFloor = 1.0f;
        }
    }
}
