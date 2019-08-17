using NWaves.Filters.Base;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Interface for any kind of filter:
    /// a filter can be applied to any signal transforming it to some output signal.
    /// </summary>
    public interface IFilter64
    {
        /// <summary>
        /// Method implements offline filtering algorithm
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="method">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto);
    }
}
