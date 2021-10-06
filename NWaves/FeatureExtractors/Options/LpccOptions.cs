using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="LpccExtractor"/>. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// Specific contracts are:
    /// <list type="bullet">
    ///     <item>FeatureCount must be positive mumber</item>
    ///     <item>LPC order must be positive mumber (usually, FeatureCount-1)</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>FrameDuration = 0.025</item>
    ///     <item>HopDuration = 0.01</item>
    ///     <item>Window = WindowType.Rectangular</item>
    ///     <item>LifterSize = 22</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class LpccOptions : LpcOptions
    {
        /// <summary>
        /// Gets or sets number of liftered coefficients (by default, 22).
        /// </summary>
        [DataMember]
        public int LifterSize { get; set; } = 22;

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (FeatureCount <= 0) errors.Add("Positive number of LPCC coefficients must be specified");
                return errors;
            }
        }
    }
}
