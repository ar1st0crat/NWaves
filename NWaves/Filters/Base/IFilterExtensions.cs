using NWaves.Signals;

namespace NWaves.Filters.Base
{
    public static class IFilterExtensions
    {
        /// <summary>
        /// Method implements online filtering (sample-by-sample, buffer-by-buffer)
        /// </summary>
        /// <param name="filter">Some filter</param>
        /// <param name="input">Input signal</param>
        /// <param name="filteringOptions">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public static DiscreteSignal Process(this IFilter filter,
                                                  DiscreteSignal input,
                                                  FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            return new DiscreteSignal(input.SamplingRate, filter.Process(input.Samples, filteringOptions));
        }
    }
}