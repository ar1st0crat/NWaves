using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Represents Discrete Cosine Transform of Type-II.
    /// </summary>
    public class Dct2 : IDct
    {
        /// <summary>
        /// DCT-II precalculated cosine matrix. 
        /// </summary>
        private readonly float[][] _dctMtx;

        /// <summary>
        /// IDCT-II precalculated cosine matrix.
        /// </summary>
        private readonly float[][] _dctMtxInv;

        /// <summary>
        /// Gets size of DCT-II.
        /// </summary>
        public int Size => _dctSize;
        private readonly int _dctSize;

        /// <summary>
        /// Constructs <see cref="Dct2"/> of given <paramref name="dctSize"/> and precalculates DCT matrices.
        /// </summary>
        /// <param name="dctSize">Size of DCT-II</param>
        public Dct2(int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new float[dctSize][];
            _dctMtxInv = new float[dctSize][];

            // Precalculate dct and idct matrices

            var m = Math.PI / (dctSize << 1);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new float[dctSize];

                for (var n = 0; n < dctSize; n++)
                {
                    _dctMtx[k][n] = 2 * (float)Math.Cos(((n << 1) + 1) * k * m);
                }
            }

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtxInv[k] = new float[dctSize];

                for (var n = 0; n < dctSize; n++)
                {
                    _dctMtxInv[k][n] = 2 * (float)Math.Cos(((k << 1) + 1) * n * m);
                }
            }
        }

        /// <summary>
        /// Does DCT-II.
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
        /// Does normalized DCT-II.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void DirectNorm(float[] input, float[] output)
        {
            var norm0 = (float)Math.Sqrt(0.5);
            var norm = (float)Math.Sqrt(0.5 / _dctSize);

            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0f;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= norm;
            }

            output[0] *= norm0;
        }

        /// <summary>
        /// Does Inverse DCT-II.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = input[0];

                for (var n = 1; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtxInv[k][n];
                }
            }
        }

        /// <summary>
        /// Does normalized Inverse DCT-II.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void InverseNorm(float[] input, float[] output)
        {
            var norm0 = (float)Math.Sqrt(0.5);
            var norm = (float)Math.Sqrt(0.5 / _dctSize);

            for (var k = 0; k < output.Length; k++)
            {
                output[k] = input[0] * _dctMtxInv[k][0] * norm0;

                for (var n = 1; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtxInv[k][n];
                }

                output[k] *= norm;
            }
        }
    }
}
