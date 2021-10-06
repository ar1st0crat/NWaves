using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring filterbank-based extractors (including MFCC, PLP, etc.). 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// Specific contracts are:
    /// <list type="bullet">
    ///     <item>Filter bank size must be positive number or the entire filter bank must be specified (not null)</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>FrameDuration = 0.025</item>
    ///     <item>HopDuration = 0.01</item>
    ///     <item>Window = WindowType.Hamming</item>
    ///     <item>FilterbankSize = 12 (acts as FeatureCount)</item>
    ///     <item>LowFrequency = 0</item>
    ///     <item>HighFrequency = 0 (i.e. it will be auto-computed as SamplingRate/2)</item>
    ///     <item>NonLinearity = NonLineriatyType.None</item>
    ///     <item>SpectrumType = SpectrumType.Power</item>
    ///     <item>LogFloor = 1.4e-45 (float epsilon)</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class FilterbankOptions : FeatureExtractorOptions
    {
        [DataMember]
        public float[][] FilterBank { get; set; }
        [DataMember]
        public int FilterBankSize { get; set; } = 12;
        [DataMember]
        public double LowFrequency { get; set; }
        [DataMember]
        public double HighFrequency { get; set; }
        [DataMember]
        public int FftSize { get; set; }
        [DataMember]
        public NonLinearityType NonLinearity { get; set; } = NonLinearityType.None;
        [DataMember]
        public SpectrumType SpectrumType { get; set; } = SpectrumType.Power;
        [DataMember]
        public float LogFloor { get; set; } = float.Epsilon;

        public FilterbankOptions() => Window = WindowType.Hamming;

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;

                if (FilterBank is null && FilterBankSize <= 0)
                {
                    errors.Add("Positive number of filters must be specified");
                }

                return errors;
            }
        }
    }
}
