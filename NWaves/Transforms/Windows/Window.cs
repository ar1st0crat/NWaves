using System;
using System.Linq;

namespace NWaves.Transforms.Windows
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
        public static double[] OfType(WindowTypes type, int length)
        {
            switch (type)
            {
                case WindowTypes.Hamming:
                    return Hamming(length);

                case WindowTypes.Blackman:
                    return Blackman(length);

                case WindowTypes.Hann:
                    return Hann(length);

                default:
                    return Rectangular(length);
            }
        }

        /// <summary>
        /// Rectangular window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Rectangular window</returns>
        public static double[] Rectangular(int length)
        {
            return Enumerable.Repeat(1.0, length).ToArray();
        }

        /// <summary>
        /// Hamming window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Hamming window</returns>
        public static double[] Hamming(int length)
        {
            var window = new double[length];
            var N = length - 1;

            for (var n = 0; n < window.Length; n++)
            {
                window[n] = 0.54 - 0.46 * Math.Cos(2 * Math.PI * n / N);
            }

            return window;
        }

        /// <summary>
        /// Blackman window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Blackman window</returns>
        public static double[] Blackman(int length)
        {
            var window = new double[length];
            var N = length - 1;

            for (var n = 0; n < window.Length; n++)
            {
                window[n] = 0.42 - 0.5 * Math.Cos(2 * Math.PI * n / N) + 0.08 * Math.Cos(4 * Math.PI * n / N);
            }

            return window;
        }

        /// <summary>
        /// Hann window
        /// </summary>
        /// <param name="length">Length of the window</param>
        /// <returns>Hann window</returns>
        public static double[] Hann(int length)
        {
            var window = new double[length];
            var N = length - 1;

            for (var n = 0; n < window.Length; n++)
            {
                window[n] = 0.5 * (1 - Math.Cos(2 * Math.PI * n / N));
            }

            return window;
        }
    }
}
