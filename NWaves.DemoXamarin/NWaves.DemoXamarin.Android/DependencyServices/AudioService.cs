using System;
using System.IO;
using System.Threading.Tasks;
using Android.Content;
using Android.Media;
using NWaves.Audio;
using NWaves.DemoXamarin.DependencyServices;
using NWaves.Effects;
using NWaves.FeatureExtractors;
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
        private readonly Encoding _audioEncodingType = Encoding.PcmFloat;

        private AudioRecord _recorder;

        private int _bufferSize;
        private byte[] _bytes;
        private bool _isRecording;

        private PitchEstimatedEventArgs _pitchArgs = new PitchEstimatedEventArgs();
        public event EventHandler<PitchEstimatedEventArgs> PitchEstimated;

        private PitchExtractor _pitchExtractor;
        private RobotEffect _robotizer;


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
            _samplingRate = Int32.Parse(audioManager.GetProperty(AudioManager.PropertyOutputSampleRate));
            _bufferSize = 4 * AudioRecord.GetMinBufferSize(_samplingRate, ChannelIn.Mono, Encoding.PcmFloat);
            _recorder = new AudioRecord(AudioSource.Mic, _samplingRate, _channelCount, _audioEncodingType, _bufferSize);
            _bytes = new byte[_bufferSize];

            _pitchExtractor = new PitchExtractor(_samplingRate, (double)_bufferSize / _samplingRate / sizeof(float));

            _robotizer = new RobotEffect(260, 1024);

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
            var sizeInFloats = _bufferSize / sizeof(float);
            var data = new float[sizeInFloats];

            var filename = TempFileName;

            using (var tempStream = new FileStream(filename, FileMode.Create))
            {
                // ==================================== main recording loop ========================================

                while (_isRecording)
                {
                    // ====================================== read data ============================================

                    await _recorder.ReadAsync(data, 0, sizeInFloats, 0);

                    // ===================================== process data ==========================================

                    _pitchArgs.PitchZcr = Pitch.FromZeroCrossingsSchmitt(data, _samplingRate);
                    _pitchArgs.PitchAutoCorr = _pitchExtractor.ProcessFrame(data)[0];

                    PitchEstimated(this, _pitchArgs); // event

                    _robotizer.Process(data, data);

                    
                    // ==================== write data to output stream (if necessary) =============================

                    Buffer.BlockCopy(data, 0, _bytes, 0, _bufferSize);      // faster than writing float-after-float
                    await tempStream.WriteAsync(_bytes, 0, _bufferSize);
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

            new Java.IO.File(TempFileName).Delete();
        }

        private string TempFileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "temp.wav");

        private string OutputFileName => Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Android/media", "recorded.wav");
    }
}
