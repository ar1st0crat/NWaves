using System.Collections.Generic;
using NWaves.Signals;

namespace NWaves.Audio.Interfaces
{
    /// <summary>
    /// Interface for audio containers.
    /// </summary>
    public interface IAudioContainer
    {
        /// <summary>
        /// Gets the list of discrete signals in container.
        /// </summary>
        List<DiscreteSignal> Signals { get; }

        /// <summary>
        /// Gets the signal from container using indexing scheme based on channel type.
        /// </summary>
        /// <param name="channel">Channel (left, right, interleave, sum, average, or ordinary index)</param>
        DiscreteSignal this[Channels channel] { get; }
    }
}
