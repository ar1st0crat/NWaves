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
        PhaseVocoder = 0,

        /// <summary>
        /// Phase vocoder with phase-locking
        /// </summary>
        PhaseVocoderPhaseLocking = 1,

        /// <summary>
        /// Waveform similarity-based Synchrnoized Overlap-Add
        /// </summary>
        Wsola = 2,

        /// <summary>
        /// Pitch-Synchronous Overlap-Add
        /// </summary>
        Psola = 3,

        /// <summary>
        /// Phase Vocoder with Synchronized Overlap-Add
        /// </summary>
        Pvsola = 4,

        /// <summary>
        /// Synchronized Overlap-Add, Fixed Synthesis
        /// </summary>
        Solafs = 5
    }
}