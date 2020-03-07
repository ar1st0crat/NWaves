using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Discrete Cosine Transform of type-IV.
    /// See https://en.wikipedia.org/wiki/Discrete_cosine_transform
    /// </summary>
    public class Dct4 : IDct
    {
        /// <summary>
        /// DCT precalculated cosine matrix
        /// </summary>
        private readonly float[][] _dctMtx;

        /// <summary>
        /// Size of DCT
        /// </summary>
        public int Size => _dctSize;
        private readonly int _dctSize;

        /// <summary>
        /// Precalculate DCT matrices
        /// </summary>
        /// <param name="dctSize"></param>
        public Dct4(int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new float[dctSize][];

            // Precalculate dct matrix

            var m = Math.PI / (dctSize << 2);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new float[dctSize];

                for (var n = 0; n < dctSize; n++)
                {
                    _dctMtx[k][n] = 2 * (float)Math.Cos(((k << 1) + 1) * ((n << 1) + 1) * m);
                }
            }
        }

        /// <summary>
        /// DCT-IV (without normalization)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void Direct(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0f;
                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// IDCT-IV (without normalization)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void Inverse(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0f;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// DCT-IV (with normalization)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void DirectNorm(float[] input, float[] output)
        {
            var norm = (float)(0.5 * Math.Sqrt(2.0 / _dctSize));

            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0f;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= norm;
            }
        }

        /// <summary>
        /// IDCT-IV (with normalization)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void InverseNorm(float[] input, float[] output)
        {
            var norm = (float)(0.5 * Math.Sqrt(2.0 / _dctSize));

            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0f;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= norm;
            }
        }
    }
}
