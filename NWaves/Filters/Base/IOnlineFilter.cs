namespace NWaves.Filters.Base
{
    /// <summary>
    /// Interface for "online" filters
    /// </summary>
    public interface IOnlineFilter
    {
        /// <summary>
        /// Method implements online filtering (sample-by-sample)
        /// </summary>
        /// <param name="input">Input sample</param>
        /// <returns>Output sample</returns>
        float Process(float input);

        /// <summary>
        /// Method for resetting the filter
        /// </summary>
        void Reset();
    }
}
