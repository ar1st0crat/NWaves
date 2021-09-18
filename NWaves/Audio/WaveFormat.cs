namespace NWaves.Audio
{
    /// <summary>
    /// PCM WAVE header structure.
    /// </summary>
    public struct WaveFormat
    {
        /// <summary>
        /// Audio format (PCM = 1).
        /// </summary>
        public short AudioFormat;

        /// <summary>
        /// Number of channels (1 - mono, 2 - stereo).
        /// </summary>
        public short ChannelCount;

        /// <summary>
        /// Sampling rate (e.g. 8000 Hz, 11025 Hz, 16000 Hz, 22050 Hz, 44100 Hz).
        /// </summary>
        public int SamplingRate;

        /// <summary>
        /// SamplingRate * NumChannels * BitsPerSample / 8.
        /// </summary>
        public int ByteRate;

        /// <summary>
        /// ChannelCount * BitsPerSample / 8.
        /// </summary>
        public short Align;

        /// <summary>
        /// Bit depth (bits per sample) (e.g. 8, 16, 24, 32).
        /// </summary>
        public short BitsPerSample;
    }
}
