using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    [DataContract]
    public class ChromaOptions : FeatureExtractorOptions
    {
        [DataMember]
        public int FftSize { get; set; }
        [DataMember]
        public double Tuning { get; set; }
        [DataMember]
        public double CenterOctave { get; set; } = 5.0;
        [DataMember]
        public double OctaveWidth { get; set; } = 2;
        [DataMember]
        public int Norm { get; set; } = 2;
        [DataMember]
        public bool BaseC { get; set; } = true;

        public ChromaOptions()
        {
            FeatureCount = 12;
            Window = WindowType.Hann;
        }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;

                if (FeatureCount <= 0)
                {
                    errors.Add("Positive number of chroma coefficients must be specified");
                }
                if (Norm < 0)
                {
                    errors.Add("Positive Norm must be specified");
                }
                if (OctaveWidth < 0)
                {
                    errors.Add("Positive octave width must be specified");
                }

                return errors;
            }
        }
    }
}
