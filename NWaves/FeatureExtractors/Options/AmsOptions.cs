using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="AmsExtractor"/>. 
    /// General contracts are:
    /// <list type="bullet">
    ///     <item>Sampling rate must be positive number</item>
    ///     <item>Frame duration must be positive number</item>
    ///     <item>Hop duration must be positive number</item>
    /// </list>
    /// <para>
    /// Default values:
    /// <list type="bullet">
    ///     <item>FrameDuration = 0.025</item>
    ///     <item>HopDuration = 0.01</item>
    ///     <item>Window = WindowType.Rectangular</item>
    ///     <item>ModulationFftSize = 64</item>
    ///     <item>ModulationHopSize = 4</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class AmsOptions : FeatureExtractorOptions
    {
        [DataMember]
        public int ModulationFftSize { get; set; } = 64;
        [DataMember]
        public int ModulationHopSize { get; set; } = 4;
        [DataMember]
        public int FftSize { get; set; }
        [DataMember]
        public IEnumerable<float[]> Featuregram { get; set; }
        [DataMember]
        public float[][] FilterBank { get; set; }
    }
}
