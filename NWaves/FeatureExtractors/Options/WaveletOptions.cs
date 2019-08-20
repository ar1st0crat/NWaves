using NWaves.FeatureExtractors.Base;

namespace NWaves.FeatureExtractors.Options
{
    public class WaveletOptions : FeatureExtractorOptions
    {
        public string WaveletName { get; set; } = "haar";
        public int FwtSize { get; set; }
        public int FwtLevel { get; set; }
    }
}
