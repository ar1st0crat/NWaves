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
            var form = new FiltersForm();
            form.ShowDialog();
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

            switch (builderComboBox.Text)
            {
                case "Sinusoid":
                    _signal2 = new SinusoidBuilder()
                                        .SetParameter("amp", 0.2)
                                        .SetParameter("freq", 200)
                                        .OfLength(sampleCount)
                                        .SampledAt(_signal1.SamplingRate)
                                        .Build();

                    builderParametersListBox.Items.Clear();
                    builderParametersListBox.Items.Add("Sinusoidal Builder:");
                    builderParametersListBox.Items.AddRange(new SinusoidBuilder().GetParametersInfo());

                    break;

                case "Sawtooth":
                    _signal2 = new SawtoothBuilder()
                                        .SetParameter("low", -0.2)
                                        .SetParameter("high", 0.5)
                                        .SetParameter("freq", 0.02)
                                        .OfLength(sampleCount)
                                        .SampledAt(_signal1.SamplingRate)
                                        .Build();

                    builderParametersListBox.Items.Clear();
                    builderParametersListBox.Items.Add("Sawtooth Builder:");
                    builderParametersListBox.Items.AddRange(new SawtoothBuilder().GetParametersInfo());

                    break;
            }

            _signal3 = _signal1.Superimpose(_signal2);

            DrawSignal(generatedSignalPanel, _signal2, 53);
            DrawSignal(superimposedSignalPanel, _signal3, 53);

            var spectrum = Transform.PowerSpectrum(_signal2[0, 512].Samples);
            DrawSignal(spectrumPanel, new DiscreteSignal(_signal2.SamplingRate, spectrum));
        }

        private void signalOperationButton_Click(object sender, EventArgs e)
        {
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

            DrawSignal(generatedSignalPanel, _signal2, 53);
            DrawSignal(superimposedSignalPanel, _signal3, 53);
        }

        private void signalSliceButton_Click(object sender, EventArgs e)
        {
            var from = int.Parse(leftSliceTextBox.Text);
            var to = int.Parse(rightSliceTextBox.Text);

            _signal2 = _signal1[from, to];

            DrawSignal(generatedSignalPanel, _signal2, 53);
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

        private void DrawSignal(Control panel, DiscreteSignal signal, int step = 1)
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
            var x = 0;

            while (i < signal.Samples.Length)
            {
                if (Math.Abs(signal[i] * 150) < panel.Height)
                {
                    g.DrawLine(pen, x, offset, x, (float)-signal[i] * 150 + offset);
                    g.DrawEllipse(pen, x - 1, (int)(-signal[i] * 150) + offset - 1, 3, 3);
                }
                x++;
                i += step;

            }

            pen.Dispose();
        }

        #endregion
    }
}
