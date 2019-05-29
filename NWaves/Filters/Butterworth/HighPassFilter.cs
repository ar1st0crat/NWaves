using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Butterworth
{
    /// <summary>
    /// Class for Butterworth IIR HP filter.
    /// </summary>
    public class HighPassFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        public HighPassFilter(double freq, int order) : base(MakeTf(freq, order))
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
            return DesignFilter.IirHpTf(freq, PrototypeButterworth.Poles(order));
        }
    }
}
