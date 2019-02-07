using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.OnePole
{
    /// <summary>
    /// Class for one-pole low-pass filter
    /// </summary>
    public class LowPassFilter : OnePoleFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        public LowPassFilter(double freq) : base(MakeTf(freq))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        public static void MakeTf(double freq, double[] b, double[] a)
        {
            a[0] = 1;
            a[1] = -Math.Exp(-2 * Math.PI * freq);

            b[0] = 1 + a[1];
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq)
        {
            var b = new double[1];
            var a = new double[2];

            MakeTf(freq, b, a);

            return new TransferFunction(b, a);
        }
    }
}
