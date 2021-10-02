using NWaves.Filters.Base;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Represents the chain of sequentially connected filters (double precision).
    /// </summary>
    public class FilterChain64 : IFilter64, IOnlineFilter64
    {
        /// <summary>
        /// List of filters in the chain.
        /// </summary>
        private readonly List<IOnlineFilter64> _filters;

        /// <summary>
        /// Constructs <see cref="FilterChain64"/> from collection of <paramref name="filters"/>.
        /// </summary>
        /// <param name="filters">Collection of online filters</param>
        public FilterChain64(IEnumerable<IOnlineFilter64> filters = null)
        {
            _filters = filters?.ToList() ?? new List<IOnlineFilter64>();
        }

        /// <summary>
        /// Constructs <see cref="FilterChain64"/> from collection of transfer functions (e.g., SOS sections). 
        /// This constructor creates objects of <see cref="IirFilter"/> under the hood.
        /// </summary>
        /// <param name="tfs">Collection of transfer functions</param>
        public FilterChain64(IEnumerable<TransferFunction> tfs)
        {
            _filters = new List<IOnlineFilter64>();

            foreach (var tf in tfs)
            {
                _filters.Add(new IirFilter64(tf));
            }
        }

        /// <summary>
        /// Adds <paramref name="filter"/> to the chain.
        /// </summary>
        /// <param name="filter">Online filter</param>
        public void Add(IOnlineFilter64 filter) => _filters.Add(filter);

        /// <summary>
        /// Inserts <paramref name="filter"/> at specified <paramref name="index"/> in the chain.
        /// </summary>
        /// <param name="index">Index of the filter in chain</param>
        /// <param name="filter">Online filter</param>
        public void Insert(int index, IOnlineFilter64 filter) => _filters.Insert(index, filter);

        /// <summary>
        /// Removes filter at specified <paramref name="index"/> from the chain.
        /// </summary>
        /// <param name="index">Index of the filter in chain</param>
        public void RemoveAt(int index) => _filters.RemoveAt(index);

        /// <summary>
        /// Processes one sample by the chain of filters.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public double Process(double sample)
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
        public double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
