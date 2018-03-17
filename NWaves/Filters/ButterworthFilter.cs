using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Class for Butterworth IIR filter.
    /// </summary>
    public class ButterworthFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        public ButterworthFilter(float freq, int order)
        {
            // Calculation of filter coefficients is based on Neil Robertson'post:
            // https://www.dsprelated.com/showarticle/1119.php

            var re = new float[order];
            var im = new float[order];
            
            var scaleFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);
                re[k] = (float)(scaleFreq * -Math.Sin(theta));
                im[k] = (float)(scaleFreq *  Math.Cos(theta));
            }

            // 2) switch to z-domain (bilinear transform)

            for (var k = 0; k < order; k++)
            {
                var den = (1 - re[k]) * (1 - re[k]) + im[k] * im[k];
                re[k] = (1 - re[k] * re[k] - im[k] * im[k]) / den;
                im[k] = 2 * im[k] / den;
            }

            // equivalent to:

            //for (var k = 0; k < order; k++)
            //{
            //      var c1 = new Complex(1 + re[k],  im[k]);
            //      var c2 = new Complex(1 - re[k], -im[k]);
            //      var c = c1 / c2;

            //      re[k] = c.Real;
            //      im[k] = c.Imaginary;
            //}

            
            // 3) polynomial coefficients

            var z = Enumerable.Repeat(-1, order).ToArray();

            B = ZpToTf(new ComplexDiscreteSignal(1, z));
            A = ZpToTf(new ComplexDiscreteSignal(1, re, im));

            var ampScale = A.Sum() / B.Sum();

            for (var i = 0; i < B.Length; i++)
            {
                B[i] *= ampScale;
            }
        }
    }
}
