using System;
using System.Linq;
using NWaves.Utils;

namespace NWaves.Windows
{
    /// <summary>
    /// Factory class for generating window coefficients of various types.
    /// </summary>
    public static class Window
    {
        /// <summary>
        /// Generate window coefficients of given <paramref name="type"/> and <paramref name="length"/>.
        /// </summary>
        /// <param name="type">Window type</param>
        /// <param name="length">Window length</param>
        /// <param name="parameters">Additional optional parameters</param>
        public static float[] OfType(WindowType type, int length, params object[] parameters)
        {
            switch (type)
            {
                case WindowType.Triangular:
                    return Triangular(length);

                case WindowType.Hamming:
                    return Hamming(length);

                case WindowType.Blackman:
                    return Blackman(length);

                case WindowType.Hann:
                    return Hann(length);

                case WindowType.Gaussian:
                    return Gaussian(length);

                case WindowType.Kaiser:
                    return parameters.Length > 0 ? Kaiser(length, (double)parameters[0]) : Kaiser(length);

                case WindowType.Kbd:
                    return parameters.Length > 0 ? Kbd(length, (double)parameters[0]) : Kbd(length);

                case WindowType.BartlettHann:
                    return BartlettHann(length);

                case WindowType.Lanczos:
                    return Lanczos(length);

                case WindowType.PowerOfSine:
                    return parameters.Length > 0 ? PowerOfSine(length, (double)parameters[0]) : PowerOfSine(length);

                case WindowType.Flattop:
                    return Flattop(length);

                case WindowType.Liftering:
                    return parameters.Length > 0 ? Liftering(length, (int)parameters[0]) : Liftering(length);

                default:
                    return Rectangular(length);
            }
        }

        /// <summary>
        /// Generate rectangular window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Rectangular(int length)
        {
            return Enumerable.Repeat(1.0f, length).ToArray();
        }

        /// <summary>
        /// Generate triangular window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Triangular(int length)
        {
            var n = length - 1;
            return Enumerable.Range(0, length)
                             .Select(i => 1.0 - 2 * Math.Abs(i - n / 2.0) / length)
                             .ToFloats();
        }

        /// <summary>
        /// Generate Hamming window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Hamming(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.54 - 0.46 * Math.Cos(i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Blackman window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Blackman(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.42 - 0.5 * Math.Cos(i * n) + 0.08 * Math.Cos(2 * i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Hann window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Hann(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.5 * (1 - Math.Cos(i * n)))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Gaussian window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Gaussian(int length)
        {
            var n = (length - 1) / 2;
            return Enumerable.Range(0, length)
                             .Select(i => Math.Exp(-0.5 * Math.Pow((i - n) / (0.4 * n), 2)))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Kaiser window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Kaiser(int length, double alpha = 12.0)
        {
            var n = 2.0 / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => MathUtils.I0(alpha * Math.Sqrt(1 - (i * n - 1) * (i * n - 1))) / MathUtils.I0(alpha))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Kaiser-Bessel Derived window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Kbd(int length, double alpha = 4.0)
        {
            var kbd = new float[length];

            var n = 4.0 / length;
            var sum = 0.0;

            for (int i = 0; i <= length / 2; i++)
            {
                sum += MathUtils.I0(Math.PI * alpha * Math.Sqrt(1 - (i * n - 1) * (i * n - 1)));
                kbd[i] = (float)sum;
            }

            for (int i = 0; i < length / 2; i++)
            {
                kbd[i] = (float)Math.Sqrt(kbd[i] / sum);
                kbd[length - 1 - i] = kbd[i];
            }

            return kbd;
        }

        /// <summary>
        /// Generate Bartlett-Hann window of given <paramref name="length"/>.
        /// </summary>
        public static float[] BartlettHann(int length)
        {
            var n = 1.0 / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.62 - 0.48 * Math.Abs(i * n - 0.5) - 0.38 * Math.Cos(2 * Math.PI * i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Lanczos window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Lanczos(int length)
        {
            var n = 2.0 / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => MathUtils.Sinc(i * n - 1))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Sin-beta window of given <paramref name="length"/>.
        /// </summary>
        public static float[] PowerOfSine(int length, double alpha = 1.5)
        {
            var n = Math.PI / length;
            return Enumerable.Range(0, length)
                             .Select(i => Math.Pow(Math.Sin(i * n), alpha))
                             .ToFloats();
        }

        /// <summary>
        /// Generate Flat-top window of given <paramref name="length"/>.
        /// </summary>
        public static float[] Flattop(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.216 - 0.417 * Math.Cos(i * n) + 0.278 * Math.Cos(2 * i * n) - 0.084 * Math.Cos(3 * i * n) + 0.007 * Math.Cos(4 * i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Generate coefficients for cepstrum liftering.
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <param name="l">Denominator in liftering formula</param>
        public static float[] Liftering(int length, int l = 22)
        {
            if (l <= 0)
            {
                return Rectangular(length);
            }
            
            return Enumerable.Range(0, length)
                             .Select(i => 1 + l * Math.Sin(Math.PI * i / l) / 2)
                             .ToFloats();
        }
    }
}
