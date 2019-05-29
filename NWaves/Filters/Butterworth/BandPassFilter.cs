using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Butterworth
{
    /// <summary>
    /// Class for Butterworth IIR BP filter.
    /// </summary>
    public class BandPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public BandPassFilter(double f1, double f2, int order) : base(MakeTf(f1, f2, order))
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
            return DesignFilter.IirBpTf(f1, f2, PrototypeButterworth.Poles(order));
        }
    }
}
