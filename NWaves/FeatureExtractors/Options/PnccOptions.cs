using NWaves.Windows;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="PnccExtractor"/>. 
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
    ///     <item>FilterbankSize = 40</item>
    ///     <item>LowFrequency = 100</item>
    ///     <item>HighFrequency = 6800</item>
    ///     <item>Power = 15</item>
    ///     <item>SpectrumType = SpectrumType.Power</item>
    ///     <item>LifterSize = 0 (no liftering)</item>
    ///     <item>LogFloor = 1.4e-45 (float epsilon)</item>
    ///     <item>LogEnergyFloor = 1.4e-45 (float epsilon)</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class PnccOptions : FilterbankOptions
    {
        [DataMember]
        public int Power { get; set; } = 15;
        [DataMember]
        public bool IncludeEnergy { get; set; }
        [DataMember]
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public PnccOptions()
        {
            LowFrequency = 100;
            HighFrequency = 6800;
            FilterBankSize = 40;
            Window = WindowType.Hamming;
        }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (FeatureCount <= 0) errors.Add("Positive number of PNCC coefficients must be specified");
                return errors;
            }
        }
    }
}
