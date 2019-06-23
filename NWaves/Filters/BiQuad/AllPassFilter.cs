using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad all-pass filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class AllPassFilter : BiQuadFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        public AllPassFilter(double freq, double q = 1) : base(MakeTf(freq, q))
        {
            Normalize();
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        private static void MakeTf(double freq, double q, double[] b, double[] a)
        {
            var omega = 2 * Math.PI * freq;
            var alpha = Math.Sin(omega) / (2 * q);
            var cosw = Math.Cos(omega);

            b[0] = 1 - alpha;
            b[1] = -2 * cosw;
            b[2] = 1 + alpha;

            a[0] = b[2];
            a[1] = b[1];
            a[2] = b[0];
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <returns>Transfer function</returns>
        private static TransferFunction MakeTf(double freq, double q)
        {
            var b = new double[3];
            var a = new double[3];

            MakeTf(freq, q, b, a);

            return new TransferFunction(b, a);
        }

        /// <summary>
        /// Change filter parameters (preserving its state)
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        public void Change(double freq, double q = 1)
        {
            MakeTf(freq, q, _b, _a);
            Normalize();
        }
    }
}