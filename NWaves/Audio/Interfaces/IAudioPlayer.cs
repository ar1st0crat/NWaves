using System.Threading.Tasks;
using NWaves.Signals;

namespace NWaves.Audio.Interfaces
{
    /// <summary>
    /// Interface for audio players.
    /// </summary>
    public interface IAudioPlayer
    {
        /// <summary>
        /// Gets or sets sound volume (usually in range [0.0f, 1.0f]).
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Play samples contained in <paramref name="signal"/> asynchronously.
        /// </summary>
        /// <param name="signal">Signal to play</param>
        /// <param name="startPos">Index of the first sample to play</param>
        /// <param name="endPos">Index of the last sample to play</param>
        /// <param name="bitDepth">Number of bits per one sample</param>
        Task PlayAsync(DiscreteSignal signal, int startPos = 0, int endPos = -1, short bitDepth = 16);

        /// <summary>
        /// Play samples contained in WAV file (or some other source) asynchronously.
        /// </summary>
        /// <param name="source">Path to WAV file (or other source) to play</param>
        /// <param name="startPos">Index of the first sample to play</param>
        /// <param name="endPos">Index of the last sample to play</param>
        Task PlayAsync(string source, int startPos = 0, int endPos = -1);

        /// <summary>
        /// Pause playing audio.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resume playing audio.
        /// </summary>
        void Resume();

        /// <summary>
        /// Stop playing audio.
        /// </summary>
        void Stop();
    }
}
