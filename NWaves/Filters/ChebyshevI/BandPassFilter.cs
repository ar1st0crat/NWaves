using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.ChebyshevI
{
    /// <summary>
    /// Band-pass Chebyshev-I filter
    /// </summary>
    public class BandPassFilter : ZiFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public BandPassFilter(double f1, double f2, int order, double ripple = 0.1) : base(MakeTf(f1, f2, order, ripple))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double f1, double f2, int order, double ripple = 0.1)
        {
            return DesignFilter.IirBpTf(f1, f2, PrototypeChebyshevI.Poles(order, ripple));
        }

        /// <summary>
        /// Change filter coeffs online
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="ripple"></param>
        public void Change(double f1, double f2, double ripple = 0.1) => Change(MakeTf(f1, f2, (_a.Length - 1) / 2, ripple));
    }
}
