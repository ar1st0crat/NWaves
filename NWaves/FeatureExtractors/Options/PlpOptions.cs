using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    public class PlpOptions : FilterbankOptions
    {
        public int LpcOrder { get; set; }    // if 0, then it'll be autocalculated as FeatureCount - 1
        public double Rasta { get; set; }
        public int LifterSize { get; set; }
        public double[] CenterFrequencies { get; set; }
        public bool IncludeEnergy { get; set; }
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public PlpOptions()
        {
            FilterBankSize = 24;
            Window = WindowTypes.Hamming;
        }
    }
}
