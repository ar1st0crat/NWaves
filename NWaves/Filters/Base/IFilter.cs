using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Interface for offline filters.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Process entire <paramref name="signal"/> and return new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto);
    }
}
