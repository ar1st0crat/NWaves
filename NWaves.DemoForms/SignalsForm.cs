using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Interfaces;
using NWaves.Audio.Mci;
using NWaves.Signals;
using NWaves.Signals.Builders;
using NWaves.Transforms;

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
        }

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
            var pitchForm = new PitchForm();
            pitchForm.ShowDialog();
        }

        private void mfccToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var mfccForm = new MfccForm();
            mfccForm.ShowDialog();
        }

        private void lpcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lpcForm = new LpcForm();
            lpcForm.ShowDialog();
        }

        private void modulationSpectrumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var msForm = new ModulationSpectrumForm();
            msForm.ShowDialog();
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

            DrawSignal(signalPanel, _signal1, 53);
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
                    signalBuilder = new SinusoidBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("amp", 0.2)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Sawtooth":
                    signalBuilder = new SawtoothBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("low", -0.3)
                                    .SetParameter("high", 0.3)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Triangle Wave":
                    signalBuilder = new TriangleWaveBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("low", -0.3)
                                    .SetParameter("high", 0.3)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Square Wave":
                    signalBuilder = new SquareWaveBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("low", -0.25)
                                    .SetParameter("high", 0.25)
                                    .SetParameter("freq", 233/*Hz*/)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Pulse Wave":
                    signalBuilder = new PulseWaveBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("amp", 0.5)
                                    .SetParameter("pulse", 0.007/*sec*/)
                                    .SetParameter("period", 0.020/*sec*/)
                                    .OfLength(sampleCount)
                                    .DelayedBy(50)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Pink Noise":
                    signalBuilder = new PinkNoiseBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.5)
                                    .SetParameter("max", 0.5)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                case "Red Noise":
                    signalBuilder = new RedNoiseBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.5)
                                    .SetParameter("max", 0.5)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;

                default:
                    signalBuilder = new WhiteNoiseBuilder();
                    _signal2 = signalBuilder
                                    .SetParameter("min", -0.5)
                                    .SetParameter("max", 0.5)
                                    .OfLength(sampleCount)
                                    .SampledAt(samplingRate)
                                    .Build();
                    break;
            }

            builderParametersListBox.Items.Clear();
            builderParametersListBox.Items.AddRange(signalBuilder.GetParametersInfo());

            if (_signal1 != null)
            {
                _signal3 = _signal1.Superimpose(_signal2);
                DrawSignal(superimposedSignalPanel, _signal3);
            }

            DrawSignal(generatedSignalPanel, _signal2);

            var spectrum = _fft.PowerSpectrum(_signal2.First(512));
            DrawSpectrum(spectrum.Samples);
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
                    _signal2 = _signal2 + param;
                    break;

                case "Repeat times":
                    _signal2 = _signal2 * param;
                    break;
            }

            _signal3 = _signal1.Superimpose(_signal2);

            DrawSignal(generatedSignalPanel, _signal2);
            DrawSignal(superimposedSignalPanel, _signal3, 53);
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

            DrawSignal(generatedSignalPanel, _signal2);
            DrawSignal(superimposedSignalPanel, _signal3, 53);
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

                DrawSignal(signalPanel, _signal1, 53);
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

        #region drawing

        private void DrawSignal(Control panel, DiscreteSignal signal, int stride = 1)
        {
            var g = panel.CreateGraphics();
            g.Clear(Color.White);

            var offset = panel.Height / 2;

            Pen pen;

            if (panel == signalPanel)
            {
                pen = new Pen(Color.Blue);
            }
            else if (panel == generatedSignalPanel)
            {
                pen = new Pen(Color.Red);
            }
            else
            {
                pen = new Pen(Color.DarkGreen);
            }

            var i = 0;
            var x = 1;

            while (i < signal.Length - stride)
            {
                var j = 0;
                var min = 0.0;
                var max = 0.0;
                while (j < stride)
                {
                    if (signal[i + j] > max) max = signal[i + j];
                    if (signal[i + j] < min) min = signal[i + j];
                    j++;
                }
                g.DrawLine(pen, x, (float)-min * 150 + offset, x, (float)-max * 150 + offset);
                x++;
                i += stride;

            }

            pen.Dispose();
        }

        private void DrawSpectrum(double[] spectrum)
        {
            var g = spectrumPanel.CreateGraphics();
            g.Clear(Color.White);

            var offset = spectrumPanel.Height - 20;

            var pen = new Pen(Color.Black);
            
            var i = 0;
            
            while (i < spectrum.Length)
            {
                g.DrawLine(pen, i, offset, i, (float)-spectrum[i] * 150 + offset);
                g.DrawEllipse(pen, i - 1, (int)(-spectrum[i] * 150) + offset - 1, 3, 3);
                i++;
            }

            pen.Dispose();
        }

        #endregion
    }
}
