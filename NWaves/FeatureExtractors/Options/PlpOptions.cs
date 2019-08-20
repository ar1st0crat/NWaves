using NWaves.FeatureExtractors.Base;
using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    public class PlpOptions : FeatureExtractorOptions
    {
        public int LpcOrder { get; set; }    // if 0, then it'll be autocalculated as NumCoefficients - 1
        public double Rasta { get; set; }
        public int FilterBankSize { get; set; } = 24;
        public double LowFrequency { get; set; }
        public double HighFrequency { get; set; }
        public int FftSize { get; set; }
        public int LifterSize { get; set; }
        public float[][] FilterBank { get; set; }
        public double[] CenterFrequencies { get; set; }

        public PlpOptions() => Window = WindowTypes.Hamming;
    }
}
