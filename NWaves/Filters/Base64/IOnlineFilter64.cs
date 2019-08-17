namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Interface for all objects that support online filtering
    /// </summary>
    public interface IOnlineFilter64
    {
        /// <summary>
        /// Method implements online filtering (sample-by-sample)
        /// </summary>
        /// <param name="input">Input sample</param>
        /// <returns>Output sample</returns>
        double Process(double input);

        /// <summary>
        /// Method for resetting state
        /// </summary>
        void Reset();
    }
}
