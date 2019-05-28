using System;
using System.Linq;
using System.Numerics;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters.Iir
{
    /// <summary>
    /// Class for Butterworth IIR LP filter.
    /// </summary>
    public class ButterworthLpFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        public ButterworthLpFilter(double freq, int order) : base(MakeTf(freq, order))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq, int order)
        {
            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1119.php

            var re = new double[order];
            var im = new double[order];

            var scaleFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);
                re[k] = scaleFreq * -Math.Sin(theta);
                im[k] = scaleFreq * Math.Cos(theta);
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

            return new TransferFunction(new ComplexDiscreteSignal(1, z),
                                        new ComplexDiscreteSignal(1, re, im),
                                        gain);
        }
    }

    /// <summary>
    /// Class for Butterworth IIR HP filter.
    /// </summary>
    public class ButterworthHpFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        public ButterworthHpFilter(double freq, int order) : base(MakeTf(freq, order))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq, int order)
        {
            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1135.php

            var re = new double[order];
            var im = new double[order];

            var scaleFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);

                var p = new Complex(-Math.Sin(theta), Math.Cos(theta));
                p = scaleFreq / p;

                re[k] = p.Real;
                im[k] = p.Imaginary;
            }

            // 2) switch to z-domain (bilinear transform)

            for (var k = 0; k < order; k++)
            {
                var den = (1 - re[k]) * (1 - re[k]) + im[k] * im[k];
                re[k] = (1 - re[k] * re[k] - im[k] * im[k]) / den;
                im[k] = 2 * im[k] / den;
            }

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

    /// <summary>
    /// Class for Butterworth IIR BP filter.
    /// </summary>
    public class ButterworthBpFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public ButterworthBpFilter(double f1, double f2, int order) : base(MakeTf(f1, f2, order))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq1, double freq2, int order)
        {
            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1128.php

            var re = new double[order * 2];
            var im = new double[order * 2];

            var f1 = Math.Tan(Math.PI * freq1);
            var f2 = Math.Tan(Math.PI * freq2);

            var f0 = Math.Sqrt(f1 * f2);
            var bw = f2 - f1;

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);

                var p = new Complex(-Math.Sin(theta), Math.Cos(theta));
                var hba = bw / 2 * p;
                var temp = Complex.Sqrt(1 - Complex.Pow(f0 / hba, 2));

                var p1 = hba * (1 + temp);
                re[k] = p1.Real;
                im[k] = p1.Imaginary;

                var p2 = hba * (1 - temp);
                re[order + k] = p2.Real;
                im[order + k] = p2.Imaginary;
            }

            // 2) switch to z-domain (bilinear transform)

            for (var k = 0; k < re.Length; k++)
            {
                var den = (1 - re[k]) * (1 - re[k]) + im[k] * im[k];
                re[k] = (1 - re[k] * re[k] - im[k] * im[k]) / den;
                im[k] = 2 * im[k] / den;
            }

            // 3) polynomial coefficients

            var z = Enumerable.Repeat(-1.0, order).Concat(Enumerable.Repeat(1.0, order)).ToArray();

            var b = TransferFunction.ZpToTf(z);
            var a = TransferFunction.ZpToTf(re, im);

            var cf = 2 * Math.PI * (freq1 + freq2) / 2;
            var w = new Complex(Math.Cos(cf), Math.Sin(cf));

            var gain = Complex.Abs(MathUtils.EvaluatePolynomial(a, w) / MathUtils.EvaluatePolynomial(b, w));

            return new TransferFunction(new ComplexDiscreteSignal(1, z),
                                        new ComplexDiscreteSignal(1, re, im),
                                        gain);
        }
    }

    /// <summary>
    /// Class for Butterworth IIR BR filter.
    /// </summary>
    public class ButterworthBrFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public ButterworthBrFilter(double f1, double f2, int order) : base(MakeTf(f1, f2, order))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq1, double freq2, int order)
        {
            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1128.php

            var re = new double[order * 2];
            var im = new double[order * 2];
            var zr = new double[order * 2];
            var zi = new double[order * 2];

            var f1 = Math.Tan(Math.PI * freq1);
            var f2 = Math.Tan(Math.PI * freq2);

            var f0 = Math.Sqrt(f1 * f2);
            var bw = f2 - f1;

            var cf = 2 * Math.PI * Math.Atan(Math.PI * f0);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);

                var p = new Complex(-Math.Sin(theta), Math.Cos(theta));
                var hba = bw / 2 / p;
                var temp = Complex.Sqrt(1 - Complex.Pow(f0 / hba, 2));

                var p1 = hba * (temp + 1);
                re[k] = p1.Real;
                im[k] = p1.Imaginary;
                zr[k] = Math.Cos(cf);
                zi[k] = Math.Sin(cf);

                var p2 = hba * (1 - temp);
                re[order + k] = p2.Real;
                im[order + k] = p2.Imaginary;
                zr[order + k] = Math.Cos(-cf);
                zi[order + k] = Math.Sin(-cf);
            }

            // 2) switch to z-domain (bilinear transform)

            for (var k = 0; k < re.Length; k++)
            {
                var den = (1 - re[k]) * (1 - re[k]) + im[k] * im[k];
                re[k] = (1 - re[k] * re[k] - im[k] * im[k]) / den;
                im[k] = 2 * im[k] / den;
            }

            // 3) polynomial coefficients

            var b = TransferFunction.ZpToTf(zr, zi);
            var a = TransferFunction.ZpToTf(re, im);

            
            var w = new Complex(Math.Cos(cf), Math.Sin(cf));

            var gain = Complex.Abs(MathUtils.EvaluatePolynomial(a, w) / MathUtils.EvaluatePolynomial(b, w));

            return new TransferFunction(new ComplexDiscreteSignal(1, zr, zi),
                                        new ComplexDiscreteSignal(1, re, im),
                                        gain);
        }
    }
}
