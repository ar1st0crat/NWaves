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
        public FilterChain(IEnumerable<IOnlineFilter> filters = null)
        {
            _filters = filters?.ToList() ?? new List<IOnlineFilter>();
        }

        /// <summary>
        /// Constructor from collection of transfer functions (e.g., SOS sections).
        /// This constructor will create IIR (!) filters.
        /// </summary>
        /// <param name="tfs"></param>
        public FilterChain(IEnumerable<TransferFunction> tfs)
        {
            _filters = new List<IOnlineFilter>();

            foreach (var tf in tfs)
            {
                _filters.Add(new IirFilter(tf));
            }
        }

        /// <summary>
        /// Add filter to the chain
        /// </summary>
        /// <param name="filter"></param>
        public void Add(IOnlineFilter filter) => _filters.Add(filter);

        /// <summary>
        /// Insert filter at specified index into the chain
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="filter"></param>
        public void Insert(int idx, IOnlineFilter filter) => _filters.Insert(idx, filter);

        /// <summary>
        /// Remove filter at specified index from the chain
        /// </summary>
        /// <param name="idx"></param>
        public void RemoveAt(int idx) => _filters.RemoveAt(idx);

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
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
