namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Algorithm for time scale modification
    /// </summary>
    public enum TsmAlgorithm
    {
        /// <summary>
        /// Phase vocoder
        /// </summary>
        PhaseVocoder,

        /// <summary>
        /// Waveform similarity-based Synchrnoized Overlap-Add
        /// </summary>
        Wsola,

        /// <summary>
        /// Pitch-Synchronous Overlap-Add
        /// </summary>
        Psola,

        /// <summary>
        /// Phase Vocoder with Synchronized Overlap-Add
        /// </summary>
        Pvsola,

        /// <summary>
        /// Synchronized Overlap-Add, Fixed Synthesis
        /// </summary>
        Solafs
    }
}