using NWaves.Windows;
using System.Collections.Generic;

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

        public virtual List<string> Errors
        {
            get
            {
                _errors.Clear();
                if (SamplingRate <= 0) _errors.Add("Sampling rate must be positive");
                if (FrameDuration <= 0) _errors.Add("Frame duration must be positive");
                if (HopDuration <= 0) _errors.Add("Hop duration must be positive");
                return _errors;
            }
        }

        private readonly List<string> _errors = new List<string>();
    }
}
