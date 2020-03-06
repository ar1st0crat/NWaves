using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Fast implementation of DCT-II via FFT
    /// </summary>
    public class FastDct2 : IDct
    {
        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly RealFft _fft;

        /// <summary>
        /// Internal temporary buffer
        /// </summary>
        private readonly float[] _temp;

        /// <summary>
        /// Size of DCT
        /// </summary>
        public int Size => _dctSize;
        private readonly int _dctSize;


        public FastDct2(int dctSize)
        {
            _dctSize = dctSize;
            _fft = new RealFft(dctSize);
            _temp = new float[dctSize];
        }

        public void Direct(float[] input, float[] output)
        {
            _fft.Direct(input, output, _temp);
            RealFft.Shift(output);
            
            //for (int i = 0; i < len; i++)
            //{
            //    vector[i] = (temp[i] * Complex.Exp(new Complex(0, -i * Math.PI / (len * 2)))).Real;
            //}
        }

        public void DirectNorm(float[] input, float[] output)
        {
            throw new NotImplementedException();
        }

        public void Inverse(float[] input, float[] output)
        {
            throw new NotImplementedException();
        }
    }
}
