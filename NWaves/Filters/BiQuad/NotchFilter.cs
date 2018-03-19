using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad notch filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class NotchFilter : IirFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        public NotchFilter(double freq, double q = 1)
        {
            var omega = 2 * Math.PI * freq;
            var alpha = Math.Sin(omega) / (2 * q);
            var cosw = Math.Cos(omega);

            var b0 = 1;
            var b1 = -2 * cosw;
            var b2 = 1;

            var a0 = 1 + alpha;
            var a1 = -2 * cosw;
            var a2 = 1 - alpha;

            B = new[] { b0, b1, b2 };
            A = new[] { a0, a1, a2 };

            Normalize();
        }
    }
}