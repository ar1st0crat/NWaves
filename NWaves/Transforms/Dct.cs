using System;

namespace NWaves.Transforms
{
    // ===== See https://en.wikipedia.org/wiki/Discrete_cosine_transform =====
    //
    // TODO: implement DCTs via FFT for large dct sizes...
    //
    // =======================================================================
    public static partial class Transform
    {
        /// <summary>
        /// Dct size used by default
        /// </summary>
        public const int DefaultDctSize = 512;

        /// <summary>
        /// DCT-I (without normalization)
        /// </summary>
        public static void Dct1(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            var sign = 1;
            var m = Math.PI / (dctSize - 1);
            for (var k = 0; k < dctSize; k++)
            {
                output[k] = (input[0] + sign * input[dctSize - 1]) / 2;
                sign = -sign;
                for (var n = 1; n < dctSize - 1; n++)
                {
                    output[k] += input[n] * Math.Cos(m * n * k);
                }
            }
        }

        /// <summary>
        /// DCT-II (without normalization)
        /// </summary>
        public static void Dct2(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            var m = Math.PI / (dctSize << 1);
            for (var k = 0; k < dctSize; k++)
            {
                output[k] = 0.0;
                for (var n = 0; n < dctSize; n++)
                {
                    output[k] += input[n] * Math.Cos(((n << 1) + 1) * k * m);
                }
            }
        }

        /// <summary>
        /// DCT-III (without normalization)
        /// </summary>
        public static void Dct3(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            var m = Math.PI / (dctSize << 1);
            for (var k = 0; k < dctSize; k++)
            {
                output[k] = input[0] * 0.5;
                for (var n = 1; n < dctSize; n++)
                {
                    output[k] += input[n] * Math.Cos(((k << 1) + 1) * n * m);
                }
            }
        }

        /// <summary>
        /// DCT-IV (without normalization)
        /// </summary>
        public static void Dct4(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            var m = Math.PI / (dctSize << 2);
            for (var k = 0; k < dctSize; k++)
            {
                output[k] = 0.0;
                for (var n = 0; n < dctSize; n++)
                {
                    output[k] += input[n] * Math.Cos(((n << 1) + 1) * ((k << 1) + 1) * m);
                }
            }
        }

        /// <summary>
        /// IDCT-I (without normalization)
        /// </summary>
        public static void Idct1(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            Dct1(input, output, dctSize);

            var coeff = 2.0 / (dctSize - 1);
            for (var i = 0; i < dctSize; i++)
            {
                output[i] *= coeff;
            }
        }

        /// <summary>
        /// IDCT-II (without normalization)
        /// </summary>
        public static void Idct2(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            Dct3(input, output, dctSize);

            var coeff = 2.0 / dctSize;
            for (var i = 0; i < dctSize; i++)
            {
                output[i] *= coeff;
            }
        }

        /// <summary>
        /// IDCT-III (without normalization)
        /// </summary>
        public static void Idct3(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            Dct2(input, output, dctSize);

            var coeff = 2.0 / dctSize;
            for (var i = 0; i < dctSize; i++)
            {
                output[i] *= coeff;
            }
        }

        /// <summary>
        /// IDCT-IV (without normalization)
        /// </summary>
        public static void Idct4(double[] input, double[] output, int dctSize = DefaultDctSize)
        {
            Dct4(input, output, dctSize);

            var coeff = 2.0 / dctSize;
            for (var i = 0; i < dctSize; i++)
            {
                output[i] *= coeff;
            }
        }
    }
}
