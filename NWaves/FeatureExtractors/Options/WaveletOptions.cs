using System.Runtime.Serialization;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines properties for configuring <see cref="WaveletExtractor"/>. 
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
    ///     <item>WaveletName = "haar"</item>
    /// </list>
    /// </para>
    /// </summary>
    [DataContract]
    public class WaveletOptions : FeatureExtractorOptions
    {
        [DataMember]
        public string WaveletName { get; set; } = "haar";
        [DataMember]
        public int FwtSize { get; set; }
        [DataMember]
        public int FwtLevel { get; set; }
    }
}
