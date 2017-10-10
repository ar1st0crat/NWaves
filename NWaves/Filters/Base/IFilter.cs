using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Interface for any kind of filter:
    /// a filter can be applied to any signal transforming it to some output signal
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Method implements filtering algorithm
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="filteringOptions">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringOptions filteringOptions);
    }
}
