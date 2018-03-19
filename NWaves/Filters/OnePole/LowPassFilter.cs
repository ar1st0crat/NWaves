using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.OnePole
{
    /// <summary>
    /// Class for one-pole low-pass filter
    /// </summary>
    public class LowPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        public LowPassFilter(double freq)
        {
            var a1 = (float)(-Math.Exp(-2 * Math.PI * freq));
            var b0 = 1 + a1;

            B = new[] { b0 };
            A = new[] { 1, a1 };
        }
    }
}
