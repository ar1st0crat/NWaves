namespace NWaves.Transforms
{
    /// <summary>
    /// Represents Discrete Cosine Transform of Type-III. 
    /// This FFT-based implementation of DCT-III is faster for bigger DCT sizes.
    /// </summary>
    public class FastDct3 : IDct
    {
        /// <summary>
        /// Internal fast DCT-II transformer.
        /// </summary>
        private readonly FastDct2 _dct2;

        /// <summary>
        /// Gets size of DCT-III.
        /// </summary>
        public int Size => _dct2.Size;

        /// <summary>
        /// Constructs <see cref="FastDct3"/> of given <paramref name="dctSize"/>.
        /// </summary>
        /// <param name="dctSize">Size of DCT-III</param>
        public FastDct3(int dctSize) => _dct2 = new FastDct2(dctSize);

        /// <summary>
        /// Does DCT-III.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(float[] input, float[] output) => _dct2.Inverse(input, output);

        /// <summary>
        /// Normalized DCT-III via FFT is not implemented.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void DirectNorm(float[] input, float[] output) => _dct2.InverseNorm(input, output);

        /// <summary>
        /// Does Inverse DCT-III.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output) => _dct2.Direct(input, output);

        /// <summary>
        /// Does normalized Inverse DCT-III.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void InverseNorm(float[] input, float[] output) => _dct2.DirectNorm(input, output);
    }
}
