using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Represents the chain of sequentially connected filters.
    /// </summary>
    public class FilterChain : IFilter, IOnlineFilter
    {
        /// <summary>
        /// List of filters in the chain.
        /// </summary>
        private readonly List<IOnlineFilter> _filters;

        /// <summary>
        /// Constructs <see cref="FilterChain"/> from collection of <paramref name="filters"/>.
        /// </summary>
        /// <param name="filters">Collection of online filters</param>
        public FilterChain(IEnumerable<IOnlineFilter> filters = null)
        {
            _filters = filters?.ToList() ?? new List<IOnlineFilter>();
        }

        /// <summary>
        /// Constructs <see cref="FilterChain"/> from collection of transfer functions (e.g., SOS sections). 
        /// This constructor creates objects of <see cref="IirFilter"/> under the hood.
        /// </summary>
        /// <param name="tfs">Collection of transfer functions</param>
        public FilterChain(IEnumerable<TransferFunction> tfs)
        {
            _filters = new List<IOnlineFilter>();

            foreach (var tf in tfs)
            {
                _filters.Add(new IirFilter(tf));
            }
        }

        /// <summary>
        /// Adds <paramref name="filter"/> to the chain.
        /// </summary>
        /// <param name="filter">Online filter</param>
        public void Add(IOnlineFilter filter) => _filters.Add(filter);

        /// <summary>
        /// Inserts <paramref name="filter"/> at specified <paramref name="index"/> in the chain.
        /// </summary>
        /// <param name="index">Index of the filter in chain</param>
        /// <param name="filter">Online filter</param>
        public void Insert(int index, IOnlineFilter filter) => _filters.Insert(index, filter);

        /// <summary>
        /// Removes filter at specified <paramref name="index"/> from the chain.
        /// </summary>
        /// <param name="index">Index of the filter in chain</param>
        public void RemoveAt(int index) => _filters.RemoveAt(index);

        /// <summary>
        /// Processes one sample by the chain of filters.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public float Process(float sample)
        {
            var output = sample;

            foreach (var filter in _filters)
            {
                output = filter.Process(output);
            }

            return output;
        }

        /// <summary>
        /// Resets all filters in the chain.
        /// </summary>
        public void Reset()
        {
            foreach (var filter in _filters)
            {
                filter.Reset();
            }
        }

        /// <summary>
        /// Applies filters to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
