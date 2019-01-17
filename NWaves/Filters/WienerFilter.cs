using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Wiener filter.
    /// Implementation is identical to sciPy.wiener().
    /// </summary>
    public class WienerFilter : IFilter
    {
        /// <summary>
        /// Size of the Wiener filter
        /// </summary>
        private readonly int _size;

        /// <summary>
        /// Estimated noise power
        /// </summary>
        private readonly double _noise;

        public WienerFilter(int size = 5, double noise = 0.0)
        {
            _size = size;
            _noise = noise;
        }

        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            throw new System.NotImplementedException();
        }
    }
}
