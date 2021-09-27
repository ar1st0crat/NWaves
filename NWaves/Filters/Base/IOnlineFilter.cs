namespace NWaves.Filters.Base
{
    /// <summary>
    /// Interface for all signal processors that support online filtering.
    /// </summary>
    public interface IOnlineFilter
    {
        /// <summary>
        /// Process one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        float Process(float sample);

        /// <summary>
        /// Reset filter.
        /// </summary>
        void Reset();
    }
}
