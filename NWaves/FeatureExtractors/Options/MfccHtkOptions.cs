using NWaves.Filters.Fda;
using NWaves.Utils;

namespace NWaves.FeatureExtractors.Options
{
    public class MfccHtkOptions : MfccOptions
    {
        public MfccHtkOptions(int samplingRate,
                              int fftSize,
                              double lowFrequency = 0,
                              double highFrequency = 0,
                              int filterbankSize = 24)
        {
            var melBands = FilterBanks.MelBands(filterbankSize, samplingRate, lowFrequency, highFrequency);
            FilterBank = FilterBanks.Triangular(fftSize, samplingRate, melBands, null, Scale.HerzToMel);
            FilterBankSize = filterbankSize;
            FftSize = fftSize;
            SamplingRate = samplingRate;
            LowFrequency = lowFrequency;
            HighFrequency = highFrequency;
            NonLinearity = NonLinearityType.LogE;
            LogFloor = 1.0f;
        }
    }
}
