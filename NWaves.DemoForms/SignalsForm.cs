using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Interfaces;
using NWaves.Audio.Mci;
using NWaves.Signals;
using NWaves.Signals.Builders;
using NWaves.Transforms;
using NWaves.Transforms.Wavelets;

namespace NWaves.DemoForms
{
    public partial class SignalsForm : Form
    {
        private DiscreteSignal _signal1;
        private DiscreteSignal _signal2;
        private DiscreteSignal _signal3;

        private string _waveFileName;

        private readonly MciAudioPlayer _player = new MciAudioPlayer();
        private bool _hasStartedPlaying;
        private bool _isPaused;

        private readonly MciAudioRecorder _recorder = new MciAudioRecorder();
        private bool _isRecording;

        private readonly Fft _fft = new Fft();

        public SignalsForm()
        {
            InitializeComponent();

            signalPanel.Gain = 100;
            generatedSignalPanel.Gain = 100;
            superimposedSignalPanel.Gain = 100;
        }

        private void OpenSignal()
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            filenameTextBox.Text = ofd.FileName;
            _waveFileName = ofd.FileName;

            using (var stream = new FileStream(_waveFileName, FileMode.Open))
            {
                IAudioContainer waveFile = new WaveFile(stream);
                _signal1 = waveFile[Channels.Left];
            }

            signalPanel.Signal = _signal1;

            Text = $"{ofd.FileName} | {_signal1.Length} samples | {_signal1.Duration} seconds";
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            OpenSignal();
        }

        private void generateSignalButton_Click(object sender, EventArgs e)
        { 
            var sampleCount = int.Parse(durationTextBox.Text);
            var samplingRate = _signal1?.SamplingRate ?? 16000;

            SignalBuilder signalBuilder;

            switch (builderComboBox.Text)
            {
                case "Sinusoid":
                    signalBuilder = new SineBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("low", -0.4f)
                                    .SetParameter("high",  0.4f)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Sawtooth":
                    signalBuilder = new SawtoothBuilder()
                                    .SetParameter("low", -0.3f)
                                    .SetParameter("high", 0.3f)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfDuration(0.3)
                                    .SampledAt(samplingRate);

                    var fadeInOut = new FadeInOutBuilder(signalBuilder).In(0.05).Out(0.12);
                    
                    _signal2 = fadeInOut.Build();
                    
                    break;

                case "Triangle Wave":
                    signalBuilder = new TriangleWaveBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("low", -0.3f)
                                    .SetParameter("high", 0.3f)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Square Wave":
                    signalBuilder = new SquareWaveBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("low", -0.25f)
                                    .SetParameter("high", 0.25f)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Pulse Wave":
                    signalBuilder = new PulseWaveBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", 0)
                                    .SetParameter("max", 0.5f)
                                    .SetParameter("pulse", 0.007f/*sec*/)
                                    .SetParameter("period", 0.020f/*sec*/)
                                    .OfLength(sampleCount)
                                    .DelayedBy(50)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Chirp":
                    signalBuilder = new ChirpBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.3f)
                                    .SetParameter("max", 0.3f)
                                    .OfLength(sampleCount)
                                    .RepeatedTimes(3)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Sinc":
                    signalBuilder = new SincBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", 0)
                                    .SetParameter("max", 0.5f)
                                    .SetParameter("freq", 700/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Ramp":
                    signalBuilder = new RampBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("slope", 0.0007f)
                                    .SetParameter("intercept", -0.5f)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "AWGN":
                    signalBuilder = new AwgnBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("sigma", 0.25f)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Pink Noise":
                    signalBuilder = new PinkNoiseBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.5f)
                                    .SetParameter("max", 0.5f)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Red Noise":
                    signalBuilder = new RedNoiseBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.5f)
                                    .SetParameter("max", 0.5f)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Perlin Noise":
                    signalBuilder = new PerlinNoiseBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.3f)
                                    .SetParameter("max", 0.7f)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                default:
                    signalBuilder = new WhiteNoiseBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.5f)
                                    .SetParameter("max", 0.5f)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;
            }

            builderParametersListBox.Items.Clear();
            builderParametersListBox.Items.AddRange(signalBuilder.GetParametersInfo());
            builderParametersListBox.Items.Add("");
            builderParametersListBox.Items.Add($"min: {_signal2.Samples.Min():F2}");
            builderParametersListBox.Items.Add($"max: {_signal2.Samples.Max():F2}");
            builderParametersListBox.Items.Add($"avg: {_signal2.Samples.Average():F4}");

            if (_signal1 != null)
            {
                //_signal3 = _signal1 + _signal2;
                var positions = Enumerable.Range(0, 3).Select(pos => pos * (_signal2.Length + 2000)).ToArray();
                _signal3 = _signal1.SuperimposeMany(_signal2, positions);
                superimposedSignalPanel.Signal = _signal3;
            }

            generatedSignalPanel.Stride = 1;
            generatedSignalPanel.Signal = _signal2;

            var spectrum = _fft.PowerSpectrum(_signal2.First(512));

            spectrumPanel.Line = spectrum.Samples;
            spectrumPanel.ToDecibel();
        }

        private void signalOperationButton_Click(object sender, EventArgs e)
        {
            if (_signal2 == null)
            {
                return;
            }

            var param = int.Parse(operationSamplesTextBox.Text);

            switch (operationComboBox.Text)
            {
                case "Delay by":
                    _signal2 = _signal2.Delay(param);
                    break;

                case "Repeat times":
                    _signal2 = _signal2.Repeat(param);
                    break;
            }

            _signal3 = _signal1 == null ? _signal2 : _signal1.Superimpose(_signal2);

            generatedSignalPanel.Signal = _signal2;
            superimposedSignalPanel.Signal = _signal3;
        }

        private void signalSliceButton_Click(object sender, EventArgs e)
        {
            if (_signal1 == null)
            {
                return;
            }

            var from = int.Parse(leftSliceTextBox.Text);
            var to = int.Parse(rightSliceTextBox.Text);

            _signal2 = _signal1[from, to];

            generatedSignalPanel.Signal = _signal2;
            superimposedSignalPanel.Signal = _signal3;
        }
        
        #region playback demo

        private async void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_waveFileName == null || _isPaused)
            {
                return;
            }

            _hasStartedPlaying = true;

            await _player.PlayAsync(_waveFileName);

            _hasStartedPlaying = false;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_waveFileName == null || _hasStartedPlaying == false)
            {
                return;
            }

            var menuItem = sender as ToolStripMenuItem;

            if (_isPaused)
            {
                _player.Resume();
                menuItem.Text = "Pause";
            }
            else
            {
                _player.Pause();
                menuItem.Text = "Resume";
            }

            _isPaused = !_isPaused;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_isPaused)
            {
                pauseToolStripMenuItem_Click(this.menuStrip1.Items[2], null);
            }

            _player.Stop();
            _hasStartedPlaying = false;
        }

        private void recordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;

            if (_isRecording)
            {
                menuItem.Text = "Record";

                _waveFileName = @"d:\recorded.wav";

                // save to recorded.wav
                _recorder.StopRecording(_waveFileName);
                
                // open it right away and load its audio contents to _signal1
                using (var stream = new FileStream(_waveFileName, FileMode.Open))
                {
                    IAudioContainer waveFile = new WaveFile(stream);
                    _signal1 = waveFile[Channels.Left];
                }

                signalPanel.Signal = _signal1;
            }
            else
            {
                menuItem.Text = "Stop rec";

                // start recording with sampling rate 16 kHz
                _recorder.StartRecording(16000);
            }

            _isRecording = !_isRecording;
        }

        #endregion

        #region menu

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenSignal();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var stream = new FileStream(sfd.FileName, FileMode.Create))
            {
                var waveFile = new WaveFile(_signal2);
                waveFile.SaveTo(stream);
            }
        }

        private void filtersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filtersForm = new FiltersForm();
            filtersForm.ShowDialog();
        }

        private void pitchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new PitchForm().ShowDialog();
        }

        private void mfccToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new MfccForm().ShowDialog();
        }

        private void lpcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new LpcForm().ShowDialog();
        }

        private void modulationSpectrumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AmsForm().ShowDialog();
        }

        private void effectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new EffectsForm().ShowDialog();
        }

        private void featuresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new FeaturesForm().ShowDialog();
        }

        private void stftButton_Click(object sender, EventArgs e)
        {
            new StftForm().ShowDialog();
        }

        private void noiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new NoiseForm().ShowDialog();
        }

        private void modulationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ModulationForm().ShowDialog();
        }

        private void onlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new OnlineDemoForm().ShowDialog();
        }

        private void adaptiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AdaptiveFiltersForm().ShowDialog();
        }

        private void hpssToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new HpssForm().ShowDialog();
        }

        private void waveletsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new WaveletForm().ShowDialog();
        }

        #endregion
    }
}
