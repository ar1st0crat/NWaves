using NWaves.Utils;
using System;
using System.Linq;
using System.Numerics;

namespace NWaves.Filters.Bessel
{
    public static class PrototypeBessel
    {
        public static double Reverse(int k, int n)
        {
            return MathUtils.Factorial(2 * n - k) /
                (Math.Pow(2, n - k) * MathUtils.Factorial(k) * MathUtils.Factorial(n - k));
        }

        public static Complex[] Poles(int order)
        {
            var a = Enumerable.Range(0, order + 1)
                              .Select(i => Reverse(order - i, order))
                              .ToArray();

            return MathUtils.PolynomialRoots(a);
        }
    }
}
