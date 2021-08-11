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
        public int FrameSize { get; set; } = 0;
        [DataMember]
        public int HopSize { get; set; } = 0;
        [DataMember]
        public double PreEmphasis { get; set; } = 0;
        [DataMember]
        public WindowType Window { get; set; } = WindowType.Rectangular;

        public virtual List<string> Errors
        {
            get
            {
                var errors = new List<string>();

                if (SamplingRate <= 0)
                {
                    errors.Add("Positive sampling rate must be specified");
                }

                if (FrameDuration <= 0 && FrameSize <= 0)
                {
                    errors.Add("Positive frame duration (in seconds) or frame size (in samples) must be specified");
                }

                if (HopDuration <= 0 && HopSize <= 0)
                {
                    errors.Add("Positive hop duration (in seconds) or hop size (in samples) must be specified");
                }

                return errors;
            }
        }
    }
}
