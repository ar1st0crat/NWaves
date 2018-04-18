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
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq)
        {
            var a1 = Math.Exp(-2 * Math.PI * (0.5 - freq));
            var b0 = 1 - a1;

            return new TransferFunction(new[] { b0 }, new[] { 1, a1 });
        }
    }
}