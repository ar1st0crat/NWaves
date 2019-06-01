using NWaves.Filters.Base;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Filters
{
    /// <summary>
    /// N-th order Thiran allpass interpolation filter for delay 'Delta' (samples)
    /// 
    /// N = 13
    /// Delta = 13 + 0.4
    /// 
    /// https://ccrma.stanford.edu/~jos/pasp/Thiran_Allpass_Interpolators.html
    /// </summary>
    public class ThiranFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="delta"></param>
        public ThiranFilter(int order, double delta) : base(MakeTf(order, delta))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="order"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static TransferFunction MakeTf(int order, double delta)
        {
            var a = Enumerable.Range(0, order + 1).Select(i => ThiranCoefficient(i, order, delta));
            var b = a.Reverse();

            return new TransferFunction(b.ToArray(), a.ToArray());
        }

        /// <summary>
        /// k-th coefficient in TF denominator
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        private static double ThiranCoefficient(int k, int n, double delta)
        {
            var a = 1.0;

            for (var i = 0; i <= n; i++)
            {
                a *= (delta - n + i) / (delta - n + k + i);
            }

            a *= Math.Pow(-1, k) * MathUtils.BinomialCoefficient(k, n);

            return a;
        }
    }
}
