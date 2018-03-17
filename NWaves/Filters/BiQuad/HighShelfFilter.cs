using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad high-shelving filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class HighShelfFilter : IirFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        public HighShelfFilter(float freq, float q = 1, float gain = 1.0f)
        {
            var a = (float)Math.Pow(10, gain / 40);
            var asqrt = (float)Math.Sqrt(a);
            var omega = 2 * Math.PI * freq;
            var alpha = (float)(Math.Sin(omega) / 2 * Math.Sqrt((a + 1 / a) * (1 / q - 1) + 2));
            var cosw = (float)Math.Cos(omega);

            var b0 = a * (a + 1 + (a - 1) * cosw + 2 * asqrt * alpha);
            var b1 = -2 * a * (a - 1 + (a + 1) * cosw);
            var b2 = a * (a + 1 + (a - 1) * cosw - 2 * asqrt * alpha);

            var a0 = a + 1 - (a - 1) * cosw + 2 * asqrt * alpha;
            var a1 = 2 * (a - 1 - (a + 1) * cosw);
            var a2 = a + 1 - (a - 1) * cosw - 2 * asqrt * alpha;

            B = new[] { b0, b1, b2 };
            A = new[] { a0, a1, a2 };

            Normalize();
        }
    }
}