namespace NWaves.Transforms
{
    /// <summary>
    /// Class representing Fast Hartley Transform.
    /// </summary>
    public class HartleyTransform
    {
        /// <summary>
        /// Gets size of Hartley transform.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Internal array for imaginary parts.
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Construct Hartley transformer. Transform <paramref name="size"/> must be a power of 2.
        /// </summary>
        /// <param name="size">Size of Hartley transform</param>
        public HartleyTransform(int size)
        {
            Size = size;
            _fft = new Fft(size);
            _im = new float[size];
        }

        /// <summary>
        /// Do Hartley transform in-place.
        /// </summary>
        /// <param name="re">Input array of samples</param>
        public void Direct(float[] re)
        {
            for (var i = 0; i < _im.Length; i++)
            {
                _im[i] = 0;
            }

            _fft.Direct(re, _im);

            for (var i = 0; i < re.Length; i++)
            {
                re[i] -= _im[i];
            }
        }

        /// <summary>
        /// Do inverse Hartley transform in-place.
        /// </summary>
        /// <param name="re">Array of input samples</param>
        public void Inverse(float[] re)
        {
            _im[0] = 0;
            
            for (var i = 1; i <= re.Length / 2; i++)
            {
                var x = (re[Size - i] - re[i]) * 0.5f;
                _im[i] = x;
                _im[Size - i] = -x;

                x = (re[i] + re[Size - i]) * 0.5f;
                re[i] = re[Size - i] = x;
            }

            _fft.Inverse(re, _im);

            for (var i = 0; i < re.Length; i++)
            {
                re[i] /= Size;
            }
        }
    }
}
