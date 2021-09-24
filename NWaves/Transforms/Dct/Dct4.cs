using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class representing Discrete Cosine Transform of Type-IV.
    /// </summary>
    public class Dct4 : IDct
    {
        /// <summary>
        /// DCT-IV precalculated cosine matrix.
        /// </summary>
        private readonly float[][] _dctMtx;

        /// <summary>
        /// Size of DCT-IV.
        /// </summary>
        public int Size => _dctSize;
        private readonly int _dctSize;

        /// <summary>
        /// Construct <see cref="Dct4"/> of given <paramref name="dctSize"/> and precalculate DCT matrices.
        /// </summary>
        /// <param name="dctSize">Size of DCT-IV</param>
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
        /// Do DCT-IV.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
        /// Do normalized DCT-IV.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
        /// Do Inverse DCT-IV.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
        /// Do normalized Inverse DCT-IV.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
