namespace NWaves.Audio.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IAudioRecorder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="samplingRate"></param>
        /// <param name="channelCount"></param>
        /// <param name="bitsPerSample"></param>
        void StartRecording(string destination, int samplingRate, short channelCount, short bitsPerSample);

        /// <summary>
        /// 
        /// </summary>
        void StopRecording();
    }
}
