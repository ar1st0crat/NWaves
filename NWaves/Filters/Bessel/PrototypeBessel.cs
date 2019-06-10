using NWaves.Utils;
using System;
using System.Linq;
using System.Numerics;

namespace NWaves.Filters.Bessel
{
    public static class PrototypeBessel
    {
        /// <summary>
        /// k-th coefficient of n-th order Bessel polynomial
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double Reverse(int k, int n)
        {
            return MathUtils.Factorial(2 * n - k) /
                (Math.Pow(2, n - k) * MathUtils.Factorial(k) * MathUtils.Factorial(n - k));
        }

        /// <summary>
        /// Analog poles of Bessel filter
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
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
