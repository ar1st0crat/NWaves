using System;

namespace NWaves.Transforms
{
    // =======================================================================
    // ===== See https://en.wikipedia.org/wiki/Discrete_cosine_transform =====
    // =======================================================================
    public class Dct
    {
        /// <summary>
        /// DCT precalculated matrices
        /// </summary>
        private double[][] _dct1;
        private double[][] _dct2;
        private double[][] _dct3;
        private double[][] _dct4;

        /// <summary>
        /// Size of DCT
        /// </summary>
        private int _dctSize;

        /// <summary>
        /// Precalculate DCT matrices
        /// </summary>
        /// <param name="length"></param>
        /// <param name="dctSize"></param>
        public void Init(int length, int dctSize)
        {
            _dct1 = new double[length][];
            _dct2 = new double[length][];
            _dct3 = new double[length][];
            _dct4 = new double[length][];

            _dctSize = dctSize;

            // DCT-1

            var m = Math.PI / (length - 1);

            for (var n = 1; n < length - 1; n++)
            {
                _dct1[n] = new double[dctSize];

                for (var k = 0; k < dctSize; k++)
                {
                    _dct1[n][k] = Math.Cos(m * n * k);
                }
            }

            // DCT-2

            m = Math.PI / (length << 1);
            
            for (var n = 0; n < length; n++)
            {
                _dct2[n] = new double[dctSize];

                for (var k = 0; k < dctSize; k++)
                {
                    _dct2[n][k] = Math.Cos(((n << 1) + 1) * k * m);
                }
            }

            // DCT-3

            for (var n = 1; n < length; n++)
            {
                _dct3[n] = new double[dctSize];

                for (var k = 0; k < dctSize; k++)
                {
                    _dct3[n][k] = Math.Cos(((k << 1) + 1) * n * m);
                }
            }

            // DCT-4

            m = Math.PI / (length << 2);

            for (var k = 0; k < length; k++)
            {
                _dct4[k] = new double[dctSize];

                for (var n = 0; n < dctSize; n++)
                {
                    _dct4[k][n] = Math.Cos(((n << 1) + 1) * ((k << 1) + 1) * m);
                }
            }
        }

        /// <summary>
        /// DCT-I (without normalization)
        /// </summary>
        public void Dct1(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = (input[0] + Math.Pow(-1, k) * input[input.Length - 1]) / 2;
            
                for (var n = 1; n < input.Length - 1; n++)
                {
                    output[k] += input[n] * _dct1[n][k];
                }
            }
        }

        /// <summary>
        /// DCT-II (without normalization)
        /// </summary>
        public void Dct2(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0;

                for (var n = 0; n < input.Length; n++)
                {
                    output[k] += input[n] * _dct2[n][k];
                }
            }
        }

        /// <summary>
        /// DCT-III (without normalization)
        /// </summary>
        public void Dct3(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = input[0] * 0.5;

                for (var n = 1; n < input.Length; n++)
                {
                    output[k] += input[n] * _dct3[n][k];
                }
            }
        }

        /// <summary>
        /// DCT-IV (without normalization)
        /// </summary>
        public void Dct4(double[] input, double[] output)
        {
            for (var k = 0; k < output.Length; k++)
            {
                output[k] = 0.0;
                for (var n = 0; n < _dctSize; n++)
                {
                    output[k] += input[n] * _dct4[n][k];
                }
            }
        }

        /// <summary>
        /// IDCT-I (without normalization)
        /// </summary>
        public void Idct1(double[] input, double[] output)
        {
            Dct1(input, output);

            var coeff = 2.0 / (_dctSize - 1);
            for (var i = 0; i < output.Length; i++)
            {
                output[i] *= coeff;
            }
        }

        /// <summary>
        /// IDCT-II (without normalization)
        /// </summary>
        public void Idct2(double[] input, double[] output)
        {
            Dct3(input, output);

            var coeff = 2.0 / _dctSize;
            for (var i = 0; i < output.Length; i++)
            {
                output[i] *= coeff;
            }
        }

        /// <summary>
        /// IDCT-III (without normalization)
        /// </summary>
        public void Idct3(double[] input, double[] output)
        {
            Dct2(input, output);

            var coeff = 2.0 / _dctSize;
            for (var i = 0; i < output.Length; i++)
            {
                output[i] *= coeff;
            }
        }

        /// <summary>
        /// IDCT-IV (without normalization)
        /// </summary>
        public void Idct4(double[] input, double[] output)
        {
            Dct4(input, output);

            var coeff = 2.0 / _dctSize;
            for (var i = 0; i < output.Length; i++)
            {
                output[i] *= coeff;
            }
        }
    }
}
