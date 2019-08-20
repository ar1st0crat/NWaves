using NWaves.Filters.Fda;

namespace NWaves.FeatureExtractors.Options
{
    public class MfccSlaneyOptions : MfccOptions
    {
        public MfccSlaneyOptions(int samplingRate,
                                 int fftSize,
                                 double lowFrequency = 0,
                                 double highFrequency = 0,
                                 int filterbankSize = 40,
                                 bool normalize = true)
        {
            FilterBank = FilterBanks.MelBankSlaney(filterbankSize, fftSize, samplingRate, lowFrequency, highFrequency, normalize);
            FilterBankSize = filterbankSize;
            FftSize = fftSize;
            SamplingRate = samplingRate;
            LowFrequency = lowFrequency;
            HighFrequency = highFrequency;
            NonLinearity = NonLinearityType.LogE;
        }
    }
}
