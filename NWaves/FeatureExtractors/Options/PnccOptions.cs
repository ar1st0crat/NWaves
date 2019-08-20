using NWaves.FeatureExtractors.Base;
using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    public class PnccOptions : FeatureExtractorOptions
    {
        public int Power { get; set; } = 15;
        public double LowFrequency { get; set; } = 100;
        public double HighFrequency { get; set; } = 6800;
        public int FilterBankSize { get; set; } = 40;
        public float[][] FilterBank { get; set; }
        public int FftSize { get; set; }

        public PnccOptions() => Window = WindowTypes.Hamming;
    }
}
