using NWaves.Filters.Fda;
using NWaves.Utils;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class MfccSlaneyOptions : MfccOptions
    {
        public MfccSlaneyOptions(int samplingRate,
                                 int featureCount,
                                 double frameDuration,
                                 double lowFrequency = 0,
                                 double highFrequency = 0,
                                 int filterbankSize = 40,
                                 int fftSize = 0,
                                 bool normalize = true)
        {
            var frameSize = (int)(frameDuration * samplingRate);
            fftSize = fftSize > frameSize ? fftSize : MathUtils.NextPowerOfTwo(frameSize);

            FilterBank = FilterBanks.MelBankSlaney(filterbankSize, fftSize, samplingRate, lowFrequency, highFrequency, normalize);
            FilterBankSize = filterbankSize;
            FeatureCount = featureCount;
            FftSize = fftSize;
            SamplingRate = samplingRate;
            LowFrequency = lowFrequency;
            HighFrequency = highFrequency;
            NonLinearity = NonLinearityType.LogE;
        }
    }
}
