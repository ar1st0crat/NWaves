using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Discrete Cosine Transform of type-III.
    /// See https://en.wikipedia.org/wiki/Discrete_cosine_transform
    /// </summary>
    public class Dct3 : IDct
    {
        /// <summary>
        /// DCT precalculated cosine matrix
        /// </summary>
        private readonly float[][] _dctMtx;

        /// <summary>
        /// IDCT precalculated cosine matrix
        /// </summary>
        private readonly float[][] _dctMtxInv;

        /// <summary>
        /// Size of DCT
        /// </summary>
        public int Size => _dctSize;
        private readonly int _dctSize;

        /// <summary>
        /// Precalculate DCT matrices
        /// </summary>
        /// <param name="dctSize"></param>
        public Dct3(int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new float[dctSize][];
            _dctMtxInv = new float[dctSize][];
            
            var m = Math.PI / (dctSize << 1);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new float[dctSize];

                for (var n = 1; n < dctSize; n++)
                {
                    _dctMtx[k][n] = 2 * (float)Math.Cos(((k << 1) + 1) * n * m);
                }
            }

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtxInv[k] = new float[dctSize];

                for (var n = 0; n < dctSize; n++)
                {
                    _dctMtxInv[k][n] = 2 * (float)Math.Cos(((n << 1) + 1) * k * m);
                }
            }
        }

        /// <summary>
        /// DCT-III (without normalization)
        /// </summary>
        public void Direct(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = input[0];

                for (var n = 1; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// DCT-III (with normalization)
        /// </summary>
        public void DirectNorm(float[] input, float[] output)
        {
            var norm0 = (float)(1 / Math.Sqrt(input.Length));
            var norm = (float)Math.Sqrt(0.5 / input.Length);

            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0f;

                for (var n = 1; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                output[k] *= norm;
                output[k] += input[0] * norm0;
            }
        }

        /// <summary>
        /// IDCT-III (without normalization)
        /// </summary>
        public void Inverse(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0f;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtxInv[k][n];
                }
            }
        }
    }
}
