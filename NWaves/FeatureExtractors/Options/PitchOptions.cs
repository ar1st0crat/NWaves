using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="PitchExtractor"/>. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// Specific contracts are:
    /// <list type="bullet">
    ///     <item>HighFrequency must be greater than LowFrequency</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>LowFrequency = 80 (Hz)</item>
    ///     <item>HighFrequency = 400 (Hz)</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class PitchOptions : FeatureExtractorOptions
    {
        [DataMember]
        public double LowFrequency { get; set; } = 80;/*Hz*/
        [DataMember]
        public double HighFrequency { get; set; } = 400;/*Hz*/

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (LowFrequency >= HighFrequency) errors.Add("Upper frequency must be greater than lower frequency");
                return errors;
            }
        }
    }
}
