using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Windows;

namespace NWaves.DemoForms
{
    public partial class StftForm : Form
    {
        private DiscreteSignal _signal;
        private DiscreteSignal _processedSignal;

        private Stft _stft;
        private List<float[]> _spectrogram;
        private WindowTypes _windowType = WindowTypes.Hann;

        private string _waveFileName;
        private short _bitDepth;

        private readonly MemoryStreamPlayer _player = new MemoryStreamPlayer();


        public StftForm()
        {
            InitializeComponent();
        }

        private void StftForm_Load(object sender, EventArgs e)
        {
            windowsComboBox.Items.AddRange(Enum.GetNames(typeof(WindowTypes)));
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _waveFileName = ofd.FileName;

            using (var stream = new FileStream(_waveFileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _bitDepth = waveFile.WaveFmt.BitsPerSample;
                _signal = waveFile[Channels.Average];
            }

            _stft = new Stft(512, 128, _windowType);

            var processed = _stft.Inverse(_stft.Direct(_signal));
            _processedSignal = new DiscreteSignal(_signal.SamplingRate, processed);

            signalPanel.Gain = 120;
            signalPanel.Signal = _signal;
            processedSignalPanel.Gain = 120;
            processedSignalPanel.Signal = _processedSignal;

            _spectrogram = _stft.Spectrogram(_signal);
            spectrogramPanel.Spectrogram = _spectrogram;
        }

        private async void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_signal, _bitDepth);
        }

        private async void play2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_processedSignal, _bitDepth);
        }

        private void windowsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _windowType = (WindowTypes)windowsComboBox.SelectedIndex;
            windowPlot.Line = Window.OfType(_windowType, 256);
        }
    }
}
