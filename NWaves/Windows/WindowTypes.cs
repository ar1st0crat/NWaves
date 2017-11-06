namespace NWaves.Windows
{
    /// <summary>
    /// Most commonly used window functions
    /// </summary>
    public enum WindowTypes
    {
        /// <summary>
        /// Reactangular window
        /// </summary>
        Rectangular,

        /// <summary>
        /// Hamming window
        /// </summary>
        Hamming,

        /// <summary>
        /// Blackman window
        /// </summary>
        Blackman,

        /// <summary>
        /// Hann window
        /// </summary>
        Hann,

        /// <summary>
        /// Window for cepstral liftering
        /// </summary>
        Liftering
    }
}