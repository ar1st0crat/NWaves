using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Discrete Cosine Transform of type-IV.
    /// See https://en.wikipedia.org/wiki/Discrete_cosine_transform
    /// </summary>
    public class Dct4
    {
        /// <summary>
        /// DCT precalculated cosine matrix
        /// </summary>
        private readonly double[][] _dctMtx;

        /// <summary>
        /// Size of DCT
        /// </summary>
        private readonly int _dctSize;

        /// <summary>
        /// Precalculate DCT matrices
        /// </summary>
        /// <param name="length"></param>
        /// <param name="dctSize"></param>
        public Dct4(int length, int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new double[dctSize][];

            // Precalculate dct matrix

            var m = Math.PI / (length << 2);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new double[length];

                for (var n = 0; n < length; n++)
                {
                    _dctMtx[k][n] = Math.Cos(((k << 1) + 1) * ((n << 1) + 1) * m);
                }
            }
        }

        /// <summary>
        /// DCT-IV (without normalization)
        /// </summary>
        public void Direct(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0;
                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// IDCT-IV (without normalization)
        /// </summary>
        public void Inverse(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= 2.0 / _dctSize;
            }
        }
    }
}
