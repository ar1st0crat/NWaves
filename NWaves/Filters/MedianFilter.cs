using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Nonlinear median filter
    /// </summary>
    class MedianFilter : IFilter
    {
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

        /// <summary>
        /// 
        /// </summary>
        public ComplexDiscreteSignal FrequencyResponse => null;

        /// <summary>
        /// 
        /// </summary>
        public DiscreteSignal ImpulseResponse => null;
    }
}
