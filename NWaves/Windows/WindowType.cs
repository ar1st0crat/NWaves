namespace NWaves.Windows
{
    /// <summary>
    /// Most commonly used window functions.
    /// </summary>
    public enum WindowType
    {
        /// <summary>
        /// Rectangular window.
        /// </summary>
        Rectangular,

        /// <summary>
        /// Triangular window.
        /// </summary>
        Triangular,

        /// <summary>
        /// Hamming window.
        /// </summary>
        Hamming,

        /// <summary>
        /// Blackman window.
        /// </summary>
        Blackman,

        /// <summary>
        /// Hann window.
        /// </summary>
        Hann,

        /// <summary>
        /// Gaussian window.
        /// </summary>
        Gaussian,

        /// <summary>
        /// Kaiser window.
        /// </summary>
        Kaiser,

        /// <summary>
        /// Kaiser-Bessel Derived window.
        /// </summary>
        Kbd,

        /// <summary>
        /// Bartlett-Hann window.
        /// </summary>
        BartlettHann,

        /// <summary>
        /// Lanczos window.
        /// </summary>
        Lanczos,

        /// <summary>
        /// Power-of-sine window.
        /// </summary>
        PowerOfSine,

        /// <summary>
        /// Flat-top window.
        /// </summary>
        Flattop,
        
        /// <summary>
        /// Window for cepstral liftering.
        /// </summary>
        Liftering
    }
}