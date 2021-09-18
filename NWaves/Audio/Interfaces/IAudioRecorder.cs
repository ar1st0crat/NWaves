namespace NWaves.Audio.Interfaces
{
    /// <summary>
    /// Interface for audio recorders.
    /// </summary>
    public interface IAudioRecorder
    {
        /// <summary>
        /// Start recording audio with specific settings.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="channelCount">Number of channels (1=mono, 2=stereo)</param>
        /// <param name="bitsPerSample">Number of bits per sample (8, 16, 24 or 32)</param>
        void StartRecording(int samplingRate, short channelCount, short bitsPerSample);

        /// <summary>
        /// Stop recording audio and save recorded sound to file or any other destination.
        /// </summary>
        /// <param name="destination">Path to output file (destination)</param>
        void StopRecording(string destination);
    }
}
