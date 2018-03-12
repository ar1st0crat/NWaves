using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Discrete Cosine Transform of type-I.
    /// See https://en.wikipedia.org/wiki/Discrete_cosine_transform
    /// </summary>
    public class Dct1
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
        public Dct1(int length, int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new double[dctSize][];

            // Precalculate dct matrix

            var m = Math.PI / (length - 1);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new double[length];

                for (var n = 1; n < length - 1; n++)
                {
                    _dctMtx[k][n] = Math.Cos(m * n * k);
                }
            }
        }

        /// <summary>
        /// DCT-I (without normalization)
        /// </summary>
        public void Direct(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = (input[0] + Math.Pow(-1, k) * input[input.Length - 1]) / 2;

                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// IDCT-I (without normalization)
        /// </summary>
        public void Inverse(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = (input[0] + Math.Pow(-1, k) * input[input.Length - 1]) / 2;

                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= 2.0 / (_dctSize - 1);
            }
        }
    }
}
