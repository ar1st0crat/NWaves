using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Discrete Cosine Transform of type-III.
    /// See https://en.wikipedia.org/wiki/Discrete_cosine_transform
    /// </summary>
    public class Dct3
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
        private readonly int _dctSize;

        /// <summary>
        /// Precalculate DCT matrices
        /// </summary>
        /// <param name="length"></param>
        /// <param name="dctSize"></param>
        public Dct3(int length, int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new float[dctSize][];
            _dctMtxInv = new float[dctSize][];
            
            var m = Math.PI / (length << 1);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new float[length];

                for (var n = 1; n < length; n++)
                {
                    _dctMtx[k][n] = (float)Math.Cos(((k << 1) + 1) * n * m);
                }
            }

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtxInv[k] = new float[length];

                for (var n = 0; n < length; n++)
                {
                    _dctMtxInv[k][n] = (float)Math.Cos(((n << 1) + 1) * k * m);
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
                output[k] = input[0] * 0.5f;

                for (var n = 1; n < input.Length; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
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

                output[k] *= 2.0f / _dctSize;
            }
        }
    }
}
