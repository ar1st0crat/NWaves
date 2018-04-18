using System;
using NWaves.Filters.Base;

namespace NWaves.Filters.OnePole
{
    /// <summary>
    /// Class for one-pole low-pass filter
    /// </summary>
    public class LowPassFilter : OnePoleFilter
    {
        /// <summary>
        /// Constructor computes the filter coefficients.
        /// </summary>
        /// <param name="freq"></param>
        public LowPassFilter(double freq) : base(MakeTf(freq))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq)
        {
            var a1 = -Math.Exp(-2 * Math.PI * freq);
            var b0 = 1 + a1;

            return new TransferFunction(new[] { b0 }, new[] { 1, a1 });
        }
    }
}
