using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Chain of filters
    /// </summary>
    public class FilterChain : IFilter, IOnlineFilter
    {
        /// <summary>
        /// List of filters in the chain
        /// </summary>
        private readonly List<IOnlineFilter> _filters;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filters"></param>
        public FilterChain(IEnumerable<IOnlineFilter> filters)
        {
            _filters = filters.ToList();
        }

        /// <summary>
        /// Add filter to the chain
        /// </summary>
        /// <param name="filter"></param>
        public void Add(IOnlineFilter filter)
        {
            _filters.Add(filter);
        }

        /// <summary>
        /// Process sample by the chain of filters
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float Process(float input)
        {
            var sample = input;

            foreach (var filter in _filters)
            {
                sample = filter.Process(sample);
            }

            return sample;
        }

        /// <summary>
        /// Reset state of all filters
        /// </summary>
        public void Reset()
        {
            foreach (var filter in _filters)
            {
                filter.Reset();
            }
        }

        /// <summary>
        /// Offline filtering
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
