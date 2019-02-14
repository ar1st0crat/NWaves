using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.OnePole
{
    /// <summary>
    /// Class for one-pole high-pass filter
    /// </summary>
    public class HighPassFilter : OnePoleFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        public HighPassFilter(double freq) : base(MakeTf(freq))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        public static void MakeTf(double freq, double[] b, double[] a)
        {
            a[0] = 1;
            a[1] = Math.Exp(-2 * Math.PI * (0.5 - freq));

            b[0] = 1 - a[1];
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

        /// <summary>
        /// Change filter parameters (preserving its state)
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        public void Change(double freq)
        {
            MakeTf(freq, _b, _a);
            Normalize();
        }
    }
}
