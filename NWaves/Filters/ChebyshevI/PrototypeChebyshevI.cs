using NWaves.Utils;
using System;
using System.Numerics;

namespace NWaves.Filters.ChebyshevI
{
    /// <summary>
    /// Chebyshev-I filter prototype.
    /// </summary>
    public static class PrototypeChebyshevI
    {
        /// <summary>
        /// Evaluates analog poles of Chebyshev-I filter of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="ripple">Ripple (in dB)</param>
        public static Complex[] Poles(int order, double ripple = 0.1)
        {
            var eps = Math.Sqrt(Math.Pow(10, ripple / 10) - 1);
            var s = MathUtils.Asinh(1 / eps) / order;
            var sinh = Math.Sinh(s);
            var cosh = Math.Cosh(s);

            var poles = new Complex[order];

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);
                var re = -sinh * Math.Sin(theta);
                var im =  cosh * Math.Cos(theta);
                poles[k] = new Complex(re, im);
            }

            return poles;
        }
    }
}
