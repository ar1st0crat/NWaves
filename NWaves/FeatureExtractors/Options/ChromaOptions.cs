using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="ChromaExtractor"/>. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// Specific contracts are:
    /// <list type="bullet">
    ///     <item>Number of chroma coefficients must be positive</item>
    ///     <item>Norm must be positive</item>
    ///     <item>Octave width must be positive</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>FrameDuration = 0.025</item>
    ///     <item>HopDuration = 0.01</item>
    ///     <item>FeatureCount = 12</item>
    ///     <item>Window = WindowType.Hann</item>
    ///     <item>OctaveWidth = 2</item>
    ///     <item>CenterOctave = 5.0</item>
    ///     <item>Norm = 2</item>
    ///     <item>BaseC = true</item>
    /// </list>
    /// </para>
    /// </summary>
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
