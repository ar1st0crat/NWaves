namespace NWaves.Audio
{
    /// <summary>
    /// Standard WAVE header
    /// </summary>
    public struct WaveFormat
    {
        /// <summary>
        /// PCM = 1
        /// </summary>
        public short AudioFormat;

        /// <summary>
        /// 1 - mono, 2 - stereo
        /// </summary>
        public short ChannelCount;

        /// <summary>
        /// 8000 Hz, 11025 Hz, 16000 Hz, 22050 Hz, 44100 Hz
        /// </summary>
        public int SamplingRate;

        /// <summary>
        /// SamplingRate * NumChannels * BitsPerSample / 8
        /// </summary>
        public int ByteRate;

        /// <summary>
        /// ChannelCount * BitsPerSample / 8
        /// </summary>
        public short Align;

        /// <summary>
        /// 8, 16, 24, 32
        /// </summary>
        public short BitsPerSample;
    }
}
