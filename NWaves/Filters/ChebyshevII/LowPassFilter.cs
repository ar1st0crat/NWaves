using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;

namespace NWaves.Filters.ChebyshevII
{
    /// <summary>
    /// Low-pass Chebyshev-II filter
    /// </summary>
    public class LowPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <param name="ripple"></param>
        public LowPassFilter(double freq, int order, double ripple = 0.1) : base(MakeTf(freq, order, ripple))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq, int order, double ripple = 0.1)
        {
            var re = new double[order];
            var im = new double[order];
            var zr = new double[order];
            var zi = new double[order];

            var scaleFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            var poles = PrototypeChebyshevII.Poles(order, ripple);
            var zeros = PrototypeChebyshevII.Zeros(order);

            for (var k = 0; k < order; k++)
            {
                var p = scaleFreq * poles[k];
                re[k] = p.Real;
                im[k] = p.Imaginary;

                var z = scaleFreq * zeros[k];
                zr[k] = z.Real;
                zi[k] = z.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(re, im);
            MathUtils.BilinearTransform(zr, zi);

            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zr, zi),
                                          new ComplexDiscreteSignal(1, re, im));
            tf.NormalizeAt(0);

            return tf;
        }
    }
}
