namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Interface for all signal processors that support online filtering (double precision).
    /// </summary>
    public interface IOnlineFilter64
    {
        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        double Process(double sample);

        /// <summary>
        /// Resets filter.
        /// </summary>
        void Reset();
    }
}
