namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Defines spectrum calculation modes.
    /// </summary>
    public enum SpectrumType
    {
        /// <summary>
        /// Sqrt(re*re + im*im).
        /// </summary>
        Magnitude,

        /// <summary>
        /// re*re + im*im.
        /// </summary>
        Power,

        /// <summary>
        /// Sqrt(re*re + im*im) / fftSize.
        /// </summary>
        MagnitudeNormalized,

        /// <summary>
        /// (re*re + im*im) / fftSize.
        /// </summary>
        PowerNormalized
    }
}
