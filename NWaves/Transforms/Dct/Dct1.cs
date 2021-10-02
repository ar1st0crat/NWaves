using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Represents Discrete Cosine Transform of Type-I.
    /// </summary>
    public class Dct1 : IDct
    {
        /// <summary>
        /// DCT-I precalculated cosine matrix. 
        /// </summary>
        private readonly float[][] _dctMtx;

        /// <summary>
        /// Gets size of DCT-I.
        /// </summary>
        public int Size => _dctSize;
        private readonly int _dctSize;

        /// <summary>
        /// Constructs <see cref="Dct1"/> of given <paramref name="dctSize"/> and precalculates DCT matrices.
        /// </summary>
         /// <param name="dctSize">Size of DCT-I</param>
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
        /// Does DCT-I.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
        /// Does normalized DCT-I.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
        /// Does Inverse DCT-I.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
        /// Does normalized Inverse DCT-I.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
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
