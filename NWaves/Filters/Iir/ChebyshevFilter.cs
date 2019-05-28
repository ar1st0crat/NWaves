using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Filters.Iir
{
    public class ChebyshevLpFilter : IirFilter
    {
        public ChebyshevLpFilter(double freq, int order, double ripple = -0.1) : base(MakeTf(freq, order, ripple))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq, int order, double ripple = -0.1)
        {
            var eps = Math.Sqrt(Math.Pow(10, -ripple / 10) - 1);
            var s = MathUtils.Asinh(1 / eps) / order;
            var sinh = -Math.Sinh(s);
            var cosh = Math.Cosh(s);

            var re = new double[order];
            var im = new double[order];

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);
                re[k] = sinh * Math.Sin(theta) * 2 * Math.PI * freq;
                im[k] = cosh * Math.Cos(theta) * 2 * Math.PI * freq;
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

            var z = Enumerable.Repeat(-1.0, order).ToArray();

            var b = TransferFunction.ZpToTf(z);
            var a = TransferFunction.ZpToTf(re, im);

            var gain = a.Sum() / b.Sum();

            for (var i = 0; i < b.Length; i++)
            {
                b[i] *= gain;
            }

            return new TransferFunction(new ComplexDiscreteSignal(1, z),
                                        new ComplexDiscreteSignal(1, re, im),
                                        gain);
        }
    }

    public class ChebyshevHpFilter : IirFilter
    {
        public ChebyshevHpFilter(double freq, int order, double ripple = -0.1) : base(MakeTf(freq, order, ripple))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq, int order, double ripple = -0.1)
        {
            var eps = Math.Sqrt(Math.Pow(10, -ripple / 10) - 1);
            var s = MathUtils.Asinh(1 / eps) / order;
            var sinh = -Math.Sinh(s);
            var cosh = Math.Cosh(s);

            var re = new double[order];
            var im = new double[order];

            var f = -1 / Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);
                re[k] = f * sinh * Math.Sin(theta);
                im[k] = f * cosh * Math.Cos(theta);
            }

            // 2) switch to z-domain (bilinear transform)

            for (var k = 0; k < order; k++)
            {
                var den = (re[k] + 1) * (re[k] + 1) + im[k] * im[k];
                re[k] = (re[k] * re[k] - 1 + im[k] * im[k]) / den;
                im[k] = 2 * im[k] / den;
            }

            // equivalent to:

            //for (var k = 0; k < order; k++)
            //{
            //      var c1 = new Complex(re[k] - 1, im[k]);
            //      var c2 = new Complex(1 + re[k], im[k]);
            //      var c = c1 / c2;

            //      re[k] = c.Real;
            //      im[k] = c.Imaginary;
            //}

            // 3) polynomial coefficients

            var z = Enumerable.Repeat(1.0, order).ToArray();

            var b = TransferFunction.ZpToTf(z);
            var a = TransferFunction.ZpToTf(re, im);

            var numGain = a.Where((e, i) => i % 2 == 0).Sum() - a.Where((e, i) => i % 2 == 1).Sum();
            var denGain = b.Where((e, i) => i % 2 == 0).Sum() - b.Where((e, i) => i % 2 == 1).Sum();
            var gain = numGain / denGain;

            return new TransferFunction(new ComplexDiscreteSignal(1, z),
                                        new ComplexDiscreteSignal(1, re, im),
                                        gain);
        }
    }
}
