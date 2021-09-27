namespace NWaves.Filters.Base
{
    /// <summary>
    /// General filtering strategies.
    /// </summary>
    public enum FilteringMethod
    {
        /// <summary>
        /// Filtering strategy is dynamically defined by NWaves library. 
        /// Usually it's the processing of each signal sample in a loop. 
        /// For longer FIR filter kernels it's the Overlap-Save algorithm.
        /// </summary>
        Auto,

        /// <summary>
        /// Filtering in time domain based on difference equations.
        /// </summary>
        DifferenceEquation,

        /// <summary>
        /// Filtering in frequency domain based on OLA algorithm.
        /// </summary>
        OverlapAdd,

        /// <summary>
        /// Filtering in frequency domain based on OLS algorithm.
        /// </summary>
        OverlapSave,

        /// <summary>
        /// Filtering strategy is defined fully in code by user.
        /// </summary>
        Custom
    }
}
