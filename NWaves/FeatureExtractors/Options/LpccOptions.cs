using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class LpccOptions : LpcOptions
    {
        [DataMember]
        public int LifterSize { get; set; } = 22;
    }
}
