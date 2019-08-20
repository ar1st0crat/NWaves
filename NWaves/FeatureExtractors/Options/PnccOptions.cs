using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    public class PnccOptions : FilterbankOptions
    {
        public int Power { get; set; } = 15;
        public bool IncludeEnergy { get; set; }
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
