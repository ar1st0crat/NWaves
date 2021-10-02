namespace NWaves.Transforms
{
    /// <summary>
    /// Represents Modified Discrete Cosine Transform (MDCT). 
    /// This FFT-based implementation of MDCT is faster for bigger DCT sizes.
    /// </summary>
    public class FastMdct : Mdct
    {
        /// <summary>
        /// Constructs <see cref="FastMdct"/> of given <paramref name="dctSize"/>.
        /// </summary>
        /// <param name="dctSize">Size of MDCT</param>
        public FastMdct(int dctSize) : base(dctSize, new FastDct4(dctSize))
        {
        }
    }
}
