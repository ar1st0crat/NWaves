using NWaves.Audio.Interfaces;

namespace NWaves.Audio.Mci
{
    /// <summary>
    /// Audio recorder based on MCI.
    /// 
    /// MciAudioRecorder works only with Windows, since it uses winmm.dll and MCI commands.
    /// </summary>
    public class MciAudioRecorder : IAudioRecorder
    {
        /// <summary>
        /// Start recording audio with specific settings
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="channelCount">Number of channels (1=mono, 2=stereo)</param>
        /// <param name="bitsPerSample">Number of bits per sample (8, 16, 24 or 32)</param>
        public void StartRecording(int samplingRate = 44100, short channelCount = 1, short bitsPerSample = 16)
        {
            var mciCommand = "open new type waveaudio alias capture";
            var result = Mci.SendString(mciCommand, null, 0, 0);

            if (result != 0)
            {
                throw new System.InvalidOperationException("Could not open device for recording!");
            }

            mciCommand = string.Format("set capture alignment {0} bitspersample {1} samplespersec {2} " +
                                       "channels {3} bytespersec {4} time format samples format tag pcm",
                                       channelCount * bitsPerSample / 8,
                                       bitsPerSample,
                                       samplingRate,
                                       channelCount,
                                       samplingRate * channelCount * bitsPerSample / 8);
            Mci.SendString(mciCommand, null, 0, 0);

            mciCommand = "record capture";
            Mci.SendString(mciCommand, null, 0, 0);
        }

        /// <summary>
        /// Stop recording audio and save it to destination WAV-file
        /// </summary>
        /// <param name="destination">Output WAV file containing recorderd sound</param>
        public void StopRecording(string destination)
        {
            var mciCommand = "stop capture";
            Mci.SendString(mciCommand, null, 0, 0);

            mciCommand = string.Format("save capture {0}", destination);
            Mci.SendString(mciCommand, null, 0, 0);

            mciCommand = "close capture";
            Mci.SendString(mciCommand, null, 0, 0);
        }
    }
}
