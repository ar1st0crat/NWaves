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
        /// Triangular window
        /// </summary>
        Triangular,

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
        /// Gaussian window
        /// </summary>
        Gaussian,
        
        /// <summary>
        /// Window for cepstral liftering
        /// </summary>
        Liftering
    }
}