namespace NWaves.Signals.Builders.Base
{
    /// <summary>
    /// Interface for online signal generators (one-sample providers).
    /// </summary>
    public interface ISampleGenerator
    {
        /// <summary>
        /// Generate new sample.
        /// </summary>
        float NextSample();

        /// <summary>
        /// Reset sample generator.
        /// </summary>
        void Reset();
    }
}
