using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="LpcExtractor"/>. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// Specific contracts are:
    /// <list type="bullet">
    ///     <item>LPC order must be positive mumber</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>FrameDuration = 0.025</item>
    ///     <item>HopDuration = 0.01</item>
    ///     <item>FeatureCount = LPC order + 1 (auto-computed)</item>
    ///     <item>Window = WindowType.Rectangular</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class LpcOptions : FeatureExtractorOptions
    {
        /// <summary>
        /// Gets or sets order of LPC. 
        /// This property is required. It has priority over FeatureCount. 
        /// FeatureCount will be autocomputed by <see cref="LpcExtractor"/> as LpcOrder+1.
        /// </summary>
        [DataMember]
        public int LpcOrder { get; set; }

        public override List<string> Errors
        {
            get
            {
                var errors = base.Errors;
                if (LpcOrder <= 0) errors.Add("Positive order of LPC must be specified");
                return errors;
            }
        }
    }
}
