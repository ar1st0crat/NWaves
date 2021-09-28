using NWaves.Utils;
using System;
using System.Linq;
using System.Numerics;

namespace NWaves.Filters.Bessel
{
    /// <summary>
    /// Bessel filter prototype.
    /// </summary>
    public static class PrototypeBessel
    {
        /// <summary>
        /// Gets <paramref name="k"/>-th coefficient of <paramref name="n"/>-th order Bessel polynomial.
        /// </summary>
        /// <param name="k">k</param>
        /// <param name="n">n</param>
        public static double Reverse(int k, int n)
        {
            return MathUtils.Factorial(2 * n - k) /
                (Math.Pow(2, n - k) * MathUtils.Factorial(k) * MathUtils.Factorial(n - k));
        }

        /// <summary>
        /// Evaluates analog poles of Bessel filter of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Filter order</param>
        public static Complex[] Poles(int order)
        {
            var a = Enumerable.Range(0, order + 1)
                              .Select(i => Reverse(order - i, order))
                              .ToArray();

            var poles = MathUtils.PolynomialRoots(a);

            // ...and normalize

            var norm = Math.Pow(10, -Math.Log10(a[order - 1]) / order);

            for (var i = 0; i < poles.Length; i++)
            {
                poles[i] *= norm;
            }

            return poles;
        }
    }
}
