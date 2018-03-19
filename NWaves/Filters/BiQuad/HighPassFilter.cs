using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad HP filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class HighPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        public HighPassFilter(double freq, double q = 1)
        {
            var omega = 2 * Math.PI * freq;
            var alpha = (float)(Math.Sin(omega) / (2 * q));
            var cosw = (float)Math.Cos(omega);

            var b0 = (1 + cosw) / 2;
            var b1 = -(1 + cosw);
            var b2 = b0;

            var a0 = 1 + alpha;
            var a1 = -2 * cosw;
            var a2 = 1 - alpha;

            B = new[] { b0, b1, b2 };
            A = new[] { a0, a1, a2 };

            Normalize();
        }
    }
}
