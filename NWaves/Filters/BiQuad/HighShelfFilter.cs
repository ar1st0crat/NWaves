using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad high-shelving filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class HighShelfFilter : BiQuadFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        public HighShelfFilter(double freq, double q = 1, double gain = 1.0) : base(MakeTf(freq, q, gain))
        {
            Normalize();
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        public static void MakeTf(double freq, double q, double gain, double[] b, double[] a)
        {
            var ga = Math.Pow(10, gain / 40);
            var asqrt = Math.Sqrt(ga);
            var omega = 2 * Math.PI * freq;
            var alpha = Math.Sin(omega) / 2 * Math.Sqrt((ga + 1 / ga) * (1 / q - 1) + 2);
            var cosw = Math.Cos(omega);

            b[0] = ga * (ga + 1 + (ga - 1) * cosw + 2 * asqrt * alpha);
            b[1] = -2 * ga * (ga - 1 + (ga + 1) * cosw);
            b[2] = ga * (ga + 1 + (ga - 1) * cosw - 2 * asqrt * alpha);

            a[0] = ga + 1 - (ga - 1) * cosw + 2 * asqrt * alpha;
            a[1] = 2 * (ga - 1 - (ga + 1) * cosw);
            a[2] = ga + 1 - (ga - 1) * cosw - 2 * asqrt * alpha;
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        /// <returns>Transfer function</returns>
        private static TransferFunction MakeTf(double freq, double q, double gain)
        {
            var b = new double[3];
            var a = new double[3];

            MakeTf(freq, q, gain, b, a);

            return new TransferFunction(b, a);
        }

        /// <summary>
        /// Change filter parameters (preserving its state)
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        public void Change(double freq, double q = 1, double gain = 1.0)
        {
            MakeTf(freq, q, gain, _b, _a);
            Normalize();
        }
    }
}