namespace NWaves.Filters.Base
{
    /// <summary>
    /// Interface for "online" filters.
    /// </summary>
    public interface IOnlineFilter
    {
        /// <summary>
        /// Method implements online filtering (buffer-by-buffer / sample-by-sample)
        /// </summary>
        /// <param name="input">Input block of samples</param>
        /// <param name="output">Block of filtered samples</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        /// <param name="method">General filtering strategy</param>
        void Process(float[] input,
                     float[] output,
                     int count,
                     int inputPos = 0,
                     int outputPos = 0,
                     FilteringMethod method = FilteringMethod.Auto);

        /// <summary>
        /// Method for resetting the filter
        /// </summary>
        void Reset();
    }
}
