using NWaves.Filters.Base;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Interface for offline filters (double precision).
    /// </summary>
    public interface IFilter64
    {
        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto);
    }
}
