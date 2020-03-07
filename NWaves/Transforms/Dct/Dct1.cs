using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Discrete Cosine Transform of type-I.
    /// See https://en.wikipedia.org/wiki/Discrete_cosine_transform
    /// </summary>
    public class Dct1 : IDct
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
        public Dct1(int dctSize)
        {
            _dctSize = dctSize;
            _dctMtx = new float[dctSize][];

            // Precalculate dct matrix

            var m = Math.PI / (dctSize - 1);

            for (var k = 0; k < dctSize; k++)
            {
                _dctMtx[k] = new float[dctSize];

                for (var n = 1; n < dctSize - 1; n++)
                {
                    _dctMtx[k][n] = 2 * (float)Math.Cos(m * n * k);
                }
            }
        }

        /// <summary>
        /// DCT-I (without normalization)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void Direct(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                if ((k & 1) == 0)
                {
                    output[k] = input[0] + input[input.Length - 1];
                }
                else
                {
                    output[k] = input[0] - input[input.Length - 1];
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
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void Inverse(float[] input, float[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                if ((k & 1) == 0)
                {
                    output[k] = (input[0] + input[input.Length - 1]);
                }
                else
                {
                    output[k] = (input[0] - input[input.Length - 1]);
                }

                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }
            }
        }

        /// <summary>
        /// DCT-I (with normalization)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void DirectNorm(float[] input, float[] output)
        {
            var sqrt2 = (float)Math.Sqrt(2);
            var norm0 = 0.5f * (float)(Math.Sqrt(1.0 / (_dctSize - 1)));
            var norm = norm0 * sqrt2;

            for (var k = 0; k < output.Length; k++)
            {
                if ((k & 1) == 0)
                {
                    output[k] = (input[0] + input[input.Length - 1]) * sqrt2;
                }
                else
                {
                    output[k] = (input[0] - input[input.Length - 1]) * sqrt2;
                }

                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                if (k > 0 && k < _dctSize - 1)
                {
                    output[k] *= norm;
                }
            }

            output[0] *= norm0;
            if (output.Length >= _dctSize) output[_dctSize - 1] *= norm0;
        }

        /// <summary>
        /// Inverse DCT-I (with normalization)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        public void InverseNorm(float[] input, float[] output)
        {
            var sqrt2 = (float)Math.Sqrt(2);
            var norm0 = 0.5f * (float)(Math.Sqrt(1.0 / (_dctSize - 1)));
            var norm = norm0 * sqrt2;

            for (var k = 0; k < output.Length; k++)
            {
                if ((k & 1) == 0)
                {
                    output[k] = (input[0] + input[input.Length - 1]) * sqrt2;
                }
                else
                {
                    output[k] = (input[0] - input[input.Length - 1]) * sqrt2;
                }

                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dctMtx[k][n];
                }

                if (k > 0 && k < _dctSize - 1)
                {
                    output[k] *= norm;
                }
            }

            output[0] *= norm0;
            if (output.Length >= _dctSize) output[_dctSize - 1] *= norm0;
        }
    }
}
