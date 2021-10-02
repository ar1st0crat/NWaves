using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Interface for offline filters.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto);
    }
}
