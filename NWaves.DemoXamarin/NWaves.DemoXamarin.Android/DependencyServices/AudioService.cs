using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using NWaves.Audio;
using NWaves.DemoXamarin.DependencyServices;
using NWaves.Effects;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Options;
using NWaves.Features;
using NWaves.Filters.Base;
using NWaves.Signals;
using Xamarin.Forms;

[assembly: Dependency(typeof(NWaves.DemoXamarin.Droid.DependencyServices.AudioService))]
namespace NWaves.DemoXamarin.Droid.DependencyServices
{
    public class AudioService : IAudioService
    {
        private int _samplingRate;

        private readonly ChannelIn _channelCount = ChannelIn.Mono;
        //private readonly Encoding _audioEncodingType = Encoding.PcmFloat;  //not available for older Android versions
        private readonly Encoding _audioEncodingType = Encoding.Pcm16bit;

        private AudioRecord _recorder;

        private int _bufferSize;
        private int _sizeInFloats;
        private byte[] _bytes, _temp;
        private float[][] _data;    // array of samples in each channel
        private bool _isRecording;

        private float[] _pitch;
        private PitchEstimatedEventArgs _pitchArgs = new PitchEstimatedEventArgs();
        public event EventHandler<PitchEstimatedEventArgs> PitchEstimated;

        private PitchExtractor _pitchExtractor;
        private IOnlineFilter _robotizer;


        public AudioService()
        {
        }

        public async void StartRecording()
        {
            if (_recorder != null)
            {
                StopRecording();
            }

            var context = Android.App.Application.Context;
            var audioManager = (AudioManager)context.GetSystemService(Context.AudioService);
            _samplingRate = int.Parse(audioManager.GetProperty(AudioManager.PropertyOutputSampleRate));

            //_bufferSize = 4 * AudioRecord.GetMinBufferSize(_samplingRate, ChannelIn.Mono, Encoding.PcmFloat);
            _bufferSize = 4 * AudioRecord.GetMinBufferSize(_samplingRate, ChannelIn.Mono, Encoding.Pcm16bit);
            _recorder = new AudioRecord(AudioSource.Mic, _samplingRate, _channelCount, _audioEncodingType, _bufferSize);

            //uncomment for PcmFloat mode: =====================
            //_sizeInFloats = _bufferSize / sizeof(float);
            //instead of Pcm16bit: =============================
            _sizeInFloats = _bufferSize / sizeof(short);
            _data = new float[1][];
            _data[0] = new float[_sizeInFloats];    // only one channel (mono)

            _bytes = new byte[_bufferSize];
            _temp = new byte[_sizeInFloats * sizeof(float)];
            

            var options = new PitchOptions
            {
                SamplingRate = _samplingRate,
                FrameDuration = (double)_sizeInFloats / _samplingRate
            };
            _pitchExtractor = new PitchExtractor(options);
            _pitch = new float[1];

            _robotizer = new RobotEffect(216, 1024);

            _recorder.StartRecording();
            _isRecording = true;

            await ProcessAudioData();
        }

        public void StopRecording()
        {
            if (_recorder == null)
            {
                return;
            }

            _isRecording = false;

            _recorder.Stop();
            _recorder.Release();
            _recorder = null;
        }

        private async Task ProcessAudioData()
        {
            var data = _data[0];
            
            var filename = TempFileName;

            using (var tempStream = new FileStream(filename, FileMode.Create))
            {
                // ==================================== main recording loop ========================================

                while (_isRecording)
                {
                    // ====================================== read data ============================================

                    //uncomment for PcmFloat mode: ============================
                    //await _recorder.ReadAsync(data, 0, _sizeInFloats, 0);
                    //instead of Pcm16bit:
                    await _recorder.ReadAsync(_bytes, 0, _bufferSize);
                    ByteConverter.ToFloats16Bit(_bytes, _data);
                    // ========================================================

                    // ===================================== process data ==========================================

                    _pitchExtractor.ProcessFrame(data, _pitch);

                    _pitchArgs.PitchZcr = Pitch.FromZeroCrossingsSchmitt(data, _samplingRate);
                    _pitchArgs.PitchAutoCorr = _pitch[0];

                    PitchEstimated(this, _pitchArgs); // event

                    _robotizer.Process(data, data);

                    // ==================== write data to output stream (if necessary) =============================

                    //uncomment for PcmFloat mode: =========================
                    //Buffer.BlockCopy(data, 0, _bytes, 0, _bufferSize);    // faster than writing float-after-float
                    //await tempStream.WriteAsync(_bytes, 0, _bufferSize);
                    //instead of Pcm16bit:
                    Buffer.BlockCopy(data, 0, _temp, 0, _temp.Length);
                    await tempStream.WriteAsync(_temp, 0, _temp.Length);
                    // =====================================================
                }
            }

            SaveToFile();
        }

        private void SaveToFile()
        {
            using (var tempStream = new FileStream(TempFileName, FileMode.Open))
            using (var br = new BinaryReader(tempStream))
            {
                var samples = new float[tempStream.Length / sizeof(float)];

                for (var i = 0; i < samples.Length; i++)
                {
                    samples[i] = br.ReadSingle();
                }

                var waveFile = new WaveFile(new DiscreteSignal(_samplingRate, samples));

                using (var outputStream = new FileStream(OutputFileName, FileMode.Create))
                {
                    waveFile.SaveTo(outputStream);
                }
            }

            using (var file = new Java.IO.File(TempFileName))
            {
                file.Delete();
            }
        }

        private string TempFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "temp.wav");

        private string OutputFileName => Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Android/media", "recorded.wav");
    }
}
