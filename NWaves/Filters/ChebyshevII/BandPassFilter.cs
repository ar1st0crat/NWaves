using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Numerics;

namespace NWaves.Filters.ChebyshevII
{
    /// <summary>
    /// Band-pass Chebyshev-II filter
    /// </summary>
    public class BandPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public BandPassFilter(double f1, double f2, int order, double ripple = -0.1) : base(MakeTf(f1, f2, order, ripple))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq1, double freq2, int order, double ripple = -0.1)
        {
            var re = new double[order * 2];
            var im = new double[order * 2];
            var zr = new double[order * 2];
            var zi = new double[order * 2];

            var centerFreq = 2 * Math.PI * (freq1 + freq2) / 2;

            var f1 = Math.Tan(Math.PI * freq1);
            var f2 = Math.Tan(Math.PI * freq2);

            var f0 = Math.Sqrt(f1 * f2);
            var bw = f2 - f1;

            // 1) zeros and poles of analog filter (scaled)

            var poles = PrototypeChebyshevII.Poles(order, ripple);
            var zeros = PrototypeChebyshevII.Zeros(order);

            for (var k = 0; k < order; k++)
            {
                var alpha = bw / 2 * poles[k];
                var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                var p1 = alpha * (1 + beta);
                re[k] = p1.Real;
                im[k] = p1.Imaginary;

                var p2 = alpha * (1 - beta);
                re[order + k] = p2.Real;
                im[order + k] = p2.Imaginary;


                alpha = bw / 2 * zeros[k];
                beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                p1 = alpha * (1 + beta);
                zr[k] = p1.Real;
                zi[k] = p1.Imaginary;

                p2 = alpha * (1 - beta);
                zr[order + k] = p2.Real;
                zi[order + k] = p2.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(re, im);
            MathUtils.BilinearTransform(zr, zi);

            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zr, zi),
                                          new ComplexDiscreteSignal(1, re, im));
            tf.NormalizeAt(centerFreq);

            return tf;
        }
    }
}
