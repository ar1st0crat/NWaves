using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Butterworth
{
    /// <summary>
    /// Class for Butterworth IIR LP filter.
    /// </summary>
    public class LowPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        public LowPassFilter(double freq, int order) : base(MakeTf(freq, order))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double freq, int order)
        {
            return DesignFilter.IirLpTf(freq, PrototypeButterworth.Poles(order));
        }
    }
}
