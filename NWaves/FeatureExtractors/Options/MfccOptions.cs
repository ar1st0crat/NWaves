using System.Collections.Generic;
using System.Runtime.Serialization;
using NWaves.Windows;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="MfccExtractor"/>. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// Specific contracts are:
    /// <list type="bullet">
    ///     <item>FeatureCount must be positive mumber</item>
    ///     <item>DCT type must have one of these values: "1", "2", "3", "4", "1N", "2N", "3N", "4N"</item>
    ///     <item>Filter bank size must be positive number or the entire filter bank must be specified (not null)</item>
    ///     <item>FeatureCount must not exceed filterbank size</item>
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
    ///     <item>DctType = "2N"</item>
    ///     <item>NonLinearity = NonLineriatyType.Log10</item>
    ///     <item>SpectrumType = SpectrumType.Power</item>
    ///     <item>LifterSize = 0 (no liftering)</item>
    ///     <item>LogFloor = 1.4e-45 (float epsilon)</item>
    ///     <item>LogEnergyFloor = 1.4e-45 (float epsilon)</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class MfccOptions : FilterbankOptions
    {
        /// <summary>
        /// Gets or sets number of liftered coefficients (0, by default, i.e. there will be no liftering).
        /// </summary>
        [DataMember]
        public int LifterSize { get; set; }

        /// <summary>
        /// Gets or sets DCT type (by default, it's normalized DCT-II, or "2N").
        /// </summary>
        [DataMember]
        public string DctType { get; set; } = "2N";

        [DataMember]
        public bool IncludeEnergy { get; set; }

        [DataMember]
        public float LogEnergyFloor { get; set; } = float.Epsilon;

        public MfccOptions()
        {
            FilterBankSize = 24;
            NonLinearity = NonLinearityType.Log10;
            Window = WindowType.Hamming;
        }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;

                if (FeatureCount <= 0)
                {
                    errors.Add("Positive number of MFCC coefficients must be specified");
                }

                if (FilterBank is null && FilterBankSize < FeatureCount ||
                    FilterBank != null && FilterBank.Length < FeatureCount)
                {
                    errors.Add("Number of coefficients must not exceed number of filters");
                }

                var dctErrorText = "Supported DCT formats: 1, 2, 3, 4, 1N, 2N, 3N, 4N";

                if (string.IsNullOrEmpty(DctType) || DctType.Length > 2)
                {
                    errors.Add(dctErrorText);
                }
                else if (!"1234".Contains(DctType.Substring(0, 1)))
                {
                    errors.Add(dctErrorText);
                }
                else if (DctType.Length == 2 && char.ToUpper(DctType[1]) != 'N')
                {
                    errors.Add(dctErrorText);
                }

                return errors;
            }
        }
    }
}
