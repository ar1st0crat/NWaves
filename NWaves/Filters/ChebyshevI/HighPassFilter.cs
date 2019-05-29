using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.ChebyshevI
{
    /// <summary>
    /// High-pass Chebyshev-I filter
    /// </summary>
    public class HighPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <param name="ripple"></param>
        public HighPassFilter(double freq, int order, double ripple = -0.1) : base(MakeTf(freq, order, ripple))
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
            return DesignFilter.IirHpTf(freq, PrototypeChebyshevI.Poles(order, ripple));
        }
    }
}
