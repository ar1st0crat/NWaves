using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="PlpExtractor"/>. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// Specific contracts are:
    /// <list type="bullet">
    ///     <item>FeatureCount must be positive mumber</item>
    ///     <item>Filter bank size must be positive number or the entire filter bank must be specified (not null)</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>FrameDuration = 0.025</item>
    ///     <item>HopDuration = 0.01</item>
    ///     <item>Window = WindowType.Hamming</item>
    ///     <item>FilterbankSize = 24</item>
    ///     <item>LowFrequency = 0</item>
    ///     <item>HighFrequency = 0 (i.e. it will be auto-computed as SamplingRate/2)</item>
    ///     <item>LpcOrder = 0 (i.e. it will be autocomputed as FeatureCount-1)</item>
    ///     <item>NonLinearity = NonLineriatyType.Log10</item>
    ///     <item>SpectrumType = SpectrumType.Power</item>
    ///     <item>LifterSize = 0 (no liftering)</item>
    ///     <item>Rasta = 0 (no RASTA-filtering)</item>
    ///     <item>LogFloor = 1.4e-45 (float epsilon)</item>
    ///     <item>LogEnergyFloor = 1.4e-45 (float epsilon)</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class PlpOptions : FilterbankOptions
    {
        /// <summary>
        /// Gets or sets order of LPC (0, by default, i.e. it will be autocomputed as FeatureCount-1).
        /// </summary>
        [DataMember]
        public int LpcOrder { get; set; }

        /// <summary>
        /// Gets or sets coefficient of RASTA-filter (0, by default, i.e. there will be no RASTA-filtering).
        /// </summary>
        [DataMember]
        public double Rasta { get; set; }

        /// <summary>
        /// Gets or sets number of liftered coefficients (0, by default, i.e. there will be no liftering).
        /// </summary>
        [DataMember]
        public int LifterSize { get; set; }
        
        [DataMember]
        public double[] CenterFrequencies { get; set; }

        [DataMember]
        public bool IncludeEnergy { get; set; }

        [DataMember]
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public PlpOptions()
        {
            FilterBankSize = 24;
            Window = WindowType.Hamming;
        }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (FeatureCount <= 0) errors.Add("Positive number of PLP coefficients must be specified");
                return errors;
            }
        }
    }
}
