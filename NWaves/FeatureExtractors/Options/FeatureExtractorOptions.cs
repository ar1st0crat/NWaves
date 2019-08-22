using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class FeatureExtractorOptions
    {
        [DataMember]
        public int FeatureCount { get; set; }
        [DataMember]
        public int SamplingRate { get; set; }
        [DataMember]
        public double FrameDuration { get; set; } = 0.025;
        [DataMember]
        public double HopDuration { get; set; } = 0.01;
        [DataMember]
        public double PreEmphasis { get; set; } = 0;
        [DataMember]
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
