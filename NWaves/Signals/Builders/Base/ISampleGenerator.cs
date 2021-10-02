namespace NWaves.Signals.Builders.Base
{
    /// <summary>
    /// Interface for online signal generators (one-sample providers).
    /// </summary>
    public interface ISampleGenerator
    {
        /// <summary>
        /// Generates new sample.
        /// </summary>
        float NextSample();

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        void Reset();
    }
}
