using System;
using NWaves.Filters.Base;
using NWaves.Operations;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Class for Butterworth IIR filter.
    /// 
    /// First, all poles are computed by formula
    /// 
    ///     poles[K] = freq * [-sin(pi * (2K+1) / 2n) + j cos(pi * (2K+1) / 2n)]
    /// 
    ///                        for K = 0, 1, ..., order - 1.
    /// 
    /// Then poles are converted to TF denominator coefficients.
    /// </summary>
    public class ButterworthFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        public ButterworthFilter(double freq, int order)
        {
            var re = new double[order];
            var im = new double[order];

            var fs = 1;//8000;
            //freq *= fs;

            for (var k = 0; k < order; k++)
            {
                var rp = freq * -Math.Sin(Math.PI * (2 * k + 1) / (2 * order));
                var ip = freq *  Math.Cos(Math.PI * (2 * k + 1) / (2 * order));

                //re[k] = Math.Exp(rp / fs) * Math.Cos(ip / fs) / order;
                //im[k] = Math.Exp(rp / fs) * Math.Sin(ip / fs) / order;
                re[k] = Math.Exp(rp * freq) * Math.Cos(ip * freq);
                im[k] = Math.Exp(rp * freq) * Math.Sin(ip * freq);
            }

            var r = new ComplexDiscreteSignal(1, new[] { 1.0, -re[0] }, new[] { 0.0, -im[0] });

            for (var k = 1; k < order; k++)
            {
                var x = new ComplexDiscreteSignal(1, new[] { 1.0, -re[k] }, new[] { 0.0, -im[k] });
                r = Operation.Convolve(r, x);
            }

            B = new[] { 1.0 };
            A = r.Real;
        }
    }
}
