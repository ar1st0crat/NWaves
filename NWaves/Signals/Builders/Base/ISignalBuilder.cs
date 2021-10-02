namespace NWaves.Signals.Builders.Base
{
    /// <summary>
    /// Interface for signal builders (offline signal generators).
    /// </summary>
    public interface ISignalBuilder
    {
        /// <summary>
        /// Gets the length of the signal to build.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Builds new signal.
        /// </summary>
        DiscreteSignal Build();
    }
}
