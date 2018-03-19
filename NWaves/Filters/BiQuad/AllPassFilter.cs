using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad all-pass filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class AllPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        public AllPassFilter(double freq, double q = 1)
        {
            var omega = 2 * Math.PI * freq;
            var alpha = (float)(Math.Sin(omega) / (2 * q));
            var cosw = (float)Math.Cos(omega);

            var b0 = 1 - alpha;
            var b1 = -2 * cosw;
            var b2 = 1 + alpha;

            var a0 = b2;
            var a1 = b1;
            var a2 = b0;

            B = new[] { b0, b1, b2 };
            A = new[] { a0, a1, a2 };

            Normalize();
        }
    }
}