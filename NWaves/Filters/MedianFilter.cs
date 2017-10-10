using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Nonlinear median filter
    /// </summary>
    public class MedianFilter : IFilter
    {
        /// <summary>
        /// 
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        public MedianFilter(int size = 9)
        {
            Size = size;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Custom)
        {
            return signal;
        }
    }
}
