using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Interface for any kind of filter
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Method implements filtering algorithm
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="filteringOptions">General filtering algorithm</param>
        /// <returns>Filtered signal</returns>
        DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringOptions filteringOptions);
        
        /// <summary>
        /// Returns (if possible) the complex frequency response of a filter
        /// </summary>
        ComplexDiscreteSignal FrequencyResponse { get; }

        /// <summary>
        /// Returns (if possible) the real-valued impulse response of a filter
        /// </summary>
        DiscreteSignal ImpulseResponse { get; }
    }
}
