namespace NWaves.Filters.Base
{
    /// <summary>
    /// General filtering strategy
    /// </summary>
    public enum FilteringOptions
    {
        /// <summary>
        /// Filtering strategy is dynamically defined by NWaves library
        /// </summary>
        Auto,

        /// <summary>
        /// Filtering in time domain based on difference equations
        /// </summary>
        DifferenceEquation,

        /// <summary>
        /// Filtering in frequency domain based on OLA algorithm
        /// </summary>
        OverlapAdd,

        /// <summary>
        /// Filtering in frequency domain based on OLS algorithm
        /// </summary>
        OverlapSave,

        /// <summary>
        /// Filtering strategy is defined fully in code by user
        /// </summary>
        Custom
    }
}
