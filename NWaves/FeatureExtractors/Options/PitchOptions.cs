using NWaves.FeatureExtractors.Base;

namespace NWaves.FeatureExtractors.Options
{
    public class PitchOptions : FeatureExtractorOptions
    {
        public double LowFrequency { get; set; } = 80;
        public double HighFrequency { get; set; } = 400;
    }
}
