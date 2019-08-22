using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class MultiFeatureOptions : FeatureExtractorOptions
    {
        [DataMember]
        public string FeatureList { get; set; } = "all";
        [DataMember]
        public int FftSize { get; set; }
        [DataMember]
        public float[] Frequencies { get; set; }
        [DataMember]
        public (double, double, double)[] FrequencyBands { get; set; }
        [DataMember]
        public Dictionary<string, object> Parameters { get; set; }
    }
}
