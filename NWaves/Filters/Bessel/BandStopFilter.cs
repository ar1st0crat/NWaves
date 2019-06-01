using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Bessel
{
    /// <summary>
    /// Band-stop Bessel filter
    /// </summary>
    public class BandStopFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public BandStopFilter(double f1, double f2, int order) : base(MakeTf(f1, f2, order))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double f1, double f2, int order)
        {
            return DesignFilter.IirBsTf(f1, f2, PrototypeBessel.Poles(order));
        }
    }
}
