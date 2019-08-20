using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    public class FeatureExtractorOptions
    {
        public int FeatureCount { get; set; }
        public int SamplingRate { get; set; }
        public double FrameDuration { get; set; } = 0.025;
        public double HopDuration { get; set; } = 0.01;
        public double PreEmphasis { get; set; } = 0;
        public WindowTypes Window { get; set; } = WindowTypes.Rectangular;

        public virtual bool IsValid => SamplingRate > 0 && FrameDuration > 0 && HopDuration > 0;
    }
}
