using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad BP filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class BandPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        public BandPassFilter(float freq, float q = 1)
        {
            var omega = 2 * Math.PI * freq;
            var alpha = (float)(Math.Sin(omega) / (2 * q));
            var cosw = (float)Math.Cos(omega);

            var b0 = alpha;
            var b1 = 0;
            var b2 = -alpha;

            var a0 = 1 + alpha;
            var a1 = -2 * cosw;
            var a2 = 1 - alpha;

            B = new[] { b0, b1, b2 };
            A = new[] { a0, a1, a2 };

            Normalize();
        }
    }
}