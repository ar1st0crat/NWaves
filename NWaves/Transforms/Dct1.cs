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
        private readonly float[][] _dctMtx;

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
            _dctMtx = new float[dctSize][];

            // Precalculate dct matrix

            var m = Math.PI / (length - 1);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new float[length];

                for (var n = 1; n < length - 1; n++)
                {
                    _dctMtx[k][n] = (float)Math.Cos(m * n * k);
                }
            }
        }

        /// <summary>
        /// DCT-I (without normalization)
        /// </summary>
        public void Direct(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                if ((k & 1) == 0)
                {
                    output[k] = (input[0] + input[input.Length - 1]) / 2;
                }
                else
                {
                    output[k] = (input[0] - input[input.Length - 1]) / 2;
                }

                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// IDCT-I (without normalization)
        /// </summary>
        public void Inverse(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                if ((k & 1) == 0)
                {
                    output[k] = (input[0] + input[input.Length - 1]) / 2;
                }
                else
                {
                    output[k] = (input[0] - input[input.Length - 1]) / 2;
                }

                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= 2.0f / (_dctSize - 1);
            }
        }
    }
}
