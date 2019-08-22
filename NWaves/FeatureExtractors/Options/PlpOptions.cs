using NWaves.Windows;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class PlpOptions : FilterbankOptions
    {
        [DataMember]
        public int LpcOrder { get; set; }    // if 0, then it'll be autocalculated as FeatureCount - 1
        [DataMember]
        public double Rasta { get; set; }
        [DataMember]
        public int LifterSize { get; set; }
        [DataMember]
        public double[] CenterFrequencies { get; set; }
        [DataMember]
        public bool IncludeEnergy { get; set; }
        [DataMember]
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public PlpOptions()
        {
            FilterBankSize = 24;
            Window = WindowTypes.Hamming;
        }
    }
}
