using System;
using System.Linq;
using NWaves.Utils;

namespace NWaves.Windows
{
    /// <summary>
    /// Factory class generating various types of window functions
    /// </summary>
    public static class Window
    {
        /// <summary>
        /// Main factory method
        /// </summary>
        /// <param name="type">Window type</param>
        /// <param name="length">Window length</param>
        /// <returns></returns>
        public static float[] OfType(WindowTypes type, int length, params object[] parameters)
        {
            switch (type)
            {
                case WindowTypes.Triangular:
                    return Triangular(length);

                case WindowTypes.Hamming:
                    return Hamming(length);

                case WindowTypes.Blackman:
                    return Blackman(length);

                case WindowTypes.Hann:
                    return Hann(length);

                case WindowTypes.Gaussian:
                    return Gaussian(length);

                case WindowTypes.Kaiser:
                    return parameters.Length > 0 ? Kaiser(length, (double)parameters[0]) : Kaiser(length);

                case WindowTypes.Kbd:
                    return parameters.Length > 0 ? Kbd(length, (double)parameters[0]) : Kbd(length);

                case WindowTypes.BartlettHann:
                    return BartlettHann(length);

                case WindowTypes.Lanczos:
                    return Lanczos(length);

                case WindowTypes.PowerOfSine:
                    return parameters.Length > 0 ? PowerOfSine(length, (double)parameters[0]) : PowerOfSine(length);

                case WindowTypes.Flattop:
                    return Flattop(length);

                case WindowTypes.Liftering:
                    return parameters.Length > 0 ? Liftering(length, (int)parameters[0]) : Liftering(length);

                default:
                    return Rectangular(length);
            }
        }

        /// <summary>
        /// Rectangular window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Rectangular window</returns>
        public static float[] Rectangular(int length)
        {
            return Enumerable.Repeat(1.0f, length).ToArray();
        }

        /// <summary>
        /// Triangular window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Triangular window</returns>
        public static float[] Triangular(int length)
        {
            var n = length - 1;
            return Enumerable.Range(0, length)
                             .Select(i => 1.0 - 2 * Math.Abs(i - n / 2.0) / length)
                             .ToFloats();
        }

        /// <summary>
        /// Hamming window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Hamming window</returns>
        public static float[] Hamming(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.54 - 0.46 * Math.Cos(i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Blackman window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Blackman window</returns>
        public static float[] Blackman(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.42 - 0.5 * Math.Cos(i * n) + 0.08 * Math.Cos(2 * i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Hann window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Hann window</returns>
        public static float[] Hann(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.5 * (1 - Math.Cos(i * n)))
                             .ToFloats();
        }

        /// <summary>
        /// Gaussian window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Gaussian window</returns>
        public static float[] Gaussian(int length)
        {
            var n = (length - 1) / 2;
            return Enumerable.Range(0, length)
                             .Select(i => Math.Exp(-0.5 * Math.Pow((i - n) / (0.4 * n), 2)))
                             .ToFloats();
        }

        /// <summary>
        /// Kaiser window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Kaiser window</returns>
        public static float[] Kaiser(int length, double alpha = 12.0)
        {
            var n = 2.0 / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => MathUtils.I0(alpha * Math.Sqrt(1 - (i * n - 1) * (i * n - 1))) / MathUtils.I0(alpha))
                             .ToFloats();
        }

        /// <summary>
        /// Kaiser-Bessel Derived window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>KBD window</returns>
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
        /// Bartlett-Hann window
        /// </summary>
        public static float[] BartlettHann(int length)
        {
            var n = 1.0 / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.62 - 0.48 * Math.Abs(i * n - 0.5) - 0.38 * Math.Cos(2 * Math.PI * i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Lanczos window
        /// </summary>
        public static float[] Lanczos(int length)
        {
            var n = 2.0 / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => MathUtils.Sinc(i * n - 1))
                             .ToFloats();
        }

        /// <summary>
        /// Sin-beta window
        /// </summary>
        public static float[] PowerOfSine(int length, double alpha = 1.5)
        {
            var n = Math.PI / length;
            return Enumerable.Range(0, length)
                             .Select(i => Math.Pow(Math.Sin(i * n), alpha))
                             .ToFloats();
        }

        /// <summary>
        /// Flat-top window
        /// </summary>
        public static float[] Flattop(int length)
        {
            var n = 2 * Math.PI / (length - 1);
            return Enumerable.Range(0, length)
                             .Select(i => 0.216 - 0.417 * Math.Cos(i * n) + 0.278 * Math.Cos(2 * i * n) - 0.084 * Math.Cos(3 * i * n) + 0.007 * Math.Cos(4 * i * n))
                             .ToFloats();
        }

        /// <summary>
        /// Simple cepstrum liftering.
        /// 
        /// Note. Indexing starts from zero (as in Kaldi, for example, and unlike HTK).
        /// 
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
