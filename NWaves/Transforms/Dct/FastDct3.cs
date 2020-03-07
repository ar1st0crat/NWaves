namespace NWaves.Transforms
{
    /// <summary>
    /// Fast implementation of DCT-III via FFT
    /// </summary>
    public class FastDct3 : IDct
    {
        /// <summary>
        /// Internal DCT-II transformer
        /// </summary>
        private readonly FastDct2 _dct2;

        /// <summary>
        /// DCT size
        /// </summary>
        public int Size => _dct2.Size;

        public FastDct3(int dctSize) => _dct2 = new FastDct2(dctSize);

        public void Direct(float[] input, float[] output) => _dct2.Inverse(input, output);

        public void DirectNorm(float[] input, float[] output) => _dct2.InverseNorm(input, output);

        public void Inverse(float[] input, float[] output) => _dct2.Direct(input, output);

        public void InverseNorm(float[] input, float[] output) => _dct2.DirectNorm(input, output);
    }
}
