using NWaves.Audio.Interfaces;

namespace NWaves.Audio.Mci
{
    /// <summary>
    /// 
    /// </summary>
    public class MciAudioRecorder : IAudioRecorder
    {
        /// <summary>
        /// 
        /// </summary>
        public WaveFormat WaveFmt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="samplingRate"></param>
        /// <param name="channelCount"></param>
        /// <param name="bitsPerSample"></param>
        public void StartRecording(string destination, int samplingRate = 44100, short channelCount = 1, short bitsPerSample = 16)
        {
            var mciCommand = string.Format("open new type waveaudio alias rec");
            Mci.SendString(mciCommand, null, 0, 0);

            // TODO...
        }

        public void StopRecording()
        {
            
        }
    }
}
