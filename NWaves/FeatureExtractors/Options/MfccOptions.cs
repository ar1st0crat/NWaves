using NWaves.FeatureExtractors.Base;
using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    public class MfccOptions : FeatureExtractorOptions
    {
        public int FilterBankSize { get; set; } = 24;
        public double LowFrequency { get; set; }
        public double HighFrequency { get; set; }
        public int FftSize { get; set; }
        public float[][] FilterBank { get; set; }
        public int LifterSize { get; set; }
        public bool IncludeEnergy { get; set; }
        public string DctType { get; set; } = "2N";
        public NonLinearityType NonLinearity { get; set; } = NonLinearityType.Log10;
        public SpectrumType SpectrumType { get; set; } = SpectrumType.Power;
        public float LogFloor { get; set; } = float.Epsilon;

        public MfccOptions() => Window = WindowTypes.Hamming;
    }
}
