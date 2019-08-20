using NWaves.FeatureExtractors.Base;
using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    public class FilterbankOptions : FeatureExtractorOptions
    {
        public float[][] FilterBank { get; set; }
        public NonLinearityType NonLinearity { get; set; } = NonLinearityType.None;
        public SpectrumType SpectrumType { get; set; } = SpectrumType.Power;
        public float LogFloor { get; set; } = float.Epsilon;

        public FilterbankOptions() => Window = WindowTypes.Hamming;
    }
}
