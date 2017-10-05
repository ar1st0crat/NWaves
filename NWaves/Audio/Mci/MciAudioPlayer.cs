using System;
using System.Text;
using System.Threading.Tasks;
using NWaves.Audio.Interfaces;
using NWaves.Signals;

namespace NWaves.Audio.Mci
{
    /// <summary>
    /// MciAudioPlayer works only with Windows, since it uses winmm.dll and MCI commands
    /// </summary>
    public class MciAudioPlayer : IAudioPlayer
    {
        /// <summary>
        /// Hidden alias for an MCI waveaudio device
        /// </summary>
        private string _alias;

        /// <summary>
        /// Volume (measured in percents from the range [0.0f, 1.0f])
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Play audio asynchronously
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public async Task PlayAsync(string source, int startPos = 0, int endPos = -1)
        {
            //Task.Run(() =>
            {
                Stop();

                _alias = Guid.NewGuid().ToString();

                var mciCommand = string.Format("open \"{0}\" type waveaudio alias {1}", source, _alias);
                Mci.SendString(mciCommand, null, 0, 0);
                
                mciCommand = string.Format("set {0} time format samples", _alias);
                Mci.SendString(mciCommand, null, 0, 0);

                var durationBuffer = new StringBuilder(255);
                mciCommand = string.Format("status {0} length", _alias);
                Mci.SendString(mciCommand, durationBuffer, 255, 0);
                var duration = int.Parse(durationBuffer.ToString());

                var samplingRateBuffer = new StringBuilder(255);
                mciCommand = string.Format("status {0} samplespersec", _alias);
                Mci.SendString(mciCommand, samplingRateBuffer, 255, 0);
                var samplingRate = int.Parse(samplingRateBuffer.ToString());

                mciCommand = string.Format("play {2} from {0} to {1} notify", startPos, endPos, _alias);
                mciCommand = mciCommand.Replace(" to -1", "");
                Mci.SendString(mciCommand, null, 0, 0);


                // ======= here's how we do asynchrony with old technology from 90's )) ========

                await Task.Delay(duration * 1000 / samplingRate);

                // ======== yes, this is the stupidest busy spin I wrote in years )) =========


                Stop();
            }//);
        }

        /// <summary>
        /// Unfortunately, MCI does not provide means for playing audio from buffers in memory.
        /// Moreover, since the library is portable, it's impossible even to write the buffer 
        /// into temporary file and play it here (it could be a workaround for the problem).
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        public Task PlayAsync(DiscreteSignal signal, int startPos = 0, int endPos = -1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Pause audio playback
        /// </summary>
        public void Pause()
        {
            if (_alias == null)
            {
                return;
            }

            var mciCommand = string.Format("pause {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);
        }

        /// <summary>
        /// Resume playing audio
        /// </summary>
        public void Resume()
        {
            if (_alias == null)
            {
                return;
            }

            var mciCommand = string.Format("resume {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);
        }

        /// <summary>
        /// Stop playing audio and close MCI device
        /// </summary>
        public void Stop()
        {
            if (_alias == null)
            {
                return;
            }

            var mciCommand = string.Format("stop {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            mciCommand = string.Format("close {0}", _alias);
            Mci.SendString(mciCommand, null, 0, 0);

            _alias = null;
        }
    }
}
