using NWaves.Windows;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class PnccOptions : FilterbankOptions
    {
        [DataMember]
        public int Power { get; set; } = 15;
        [DataMember]
        public bool IncludeEnergy { get; set; }
        [DataMember]
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public PnccOptions()
        {
            LowFrequency = 100;
            HighFrequency = 6800;
            FilterBankSize = 40;
            Window = WindowTypes.Hamming;
        }
    }
}
