using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Options
{
    public class MultiFeatureOptions : FeatureExtractorOptions
    {
        public string FeatureList { get; set; } = "all";
        public int FftSize { get; set; }
        public IReadOnlyDictionary<string, object> Parameters { get; set; }
        public float[] Frequencies { get; set; }
        public (double, double, double)[] FrequencyBands { get; set; }
    }
}
