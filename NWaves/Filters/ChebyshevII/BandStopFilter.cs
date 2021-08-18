using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.ChebyshevII
{
    /// <summary>
    /// Band-stop Chebyshev-II filter
    /// </summary>
    public class BandStopFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public BandStopFilter(double f1, double f2, int order, double ripple = 0.1) : base(MakeTf(f1, f2, order, ripple))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq1, double freq2, int order, double ripple = 0.1)
        {
            return DesignFilter.IirBsTf(freq1, freq2,
                                        PrototypeChebyshevII.Poles(order, ripple),
                                        PrototypeChebyshevII.Zeros(order));
        }

        /// <summary>
        /// Change filter coeffs online
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="ripple"></param>
        public void Change(double f1, double f2, double ripple = 0.1) => Change(MakeTf(f1, f2, _b.Length / 2, ripple));
    }
}
