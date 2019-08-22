using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class AmsOptions : FeatureExtractorOptions
    {
        [DataMember]
        public int ModulationFftSize { get; set; } = 64;
        [DataMember]
        public int ModulationHopSize { get; set; } = 4;
        [DataMember]
        public int FftSize { get; set; }
        [DataMember]
        public IEnumerable<float[]> Featuregram { get; set; }
        [DataMember]
        public float[][] FilterBank { get; set; }
    }
}
