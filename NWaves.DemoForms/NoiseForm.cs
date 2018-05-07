using System;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Operations;
using NWaves.Signals;

namespace NWaves.DemoForms
{
    public partial class NoiseForm : Form
    {
        private DiscreteSignal _signal;
        private DiscreteSignal _noise;
        private DiscreteSignal _processed;

        private short _bitDepth;

        private readonly MemoryStreamPlayer _player = new MemoryStreamPlayer();


        public NoiseForm()
        {
            InitializeComponent();

            signalPlot.Gain = 100;
            noisePlot.Gain = 100;
            processedPlot.Gain = 100;
        }

        private void loadsignalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"D:\Docs\Research\DATABASE\Dictor1\wav";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var stream = new FileStream(ofd.FileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _bitDepth = waveFile.WaveFmt.BitsPerSample;
                _signal = waveFile[Channels.Average];
            }
            
            signalPlot.Signal = _signal;
        }

        private void loadnoiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"D:\Docs\Research\DATABASE\Various\Фоновые звуки";
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var stream = new FileStream(ofd.FileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _bitDepth = waveFile.WaveFmt.BitsPerSample;
                _noise = waveFile[Channels.Average];
            }

            _noise.Amplify(0.2f);

            noisePlot.Signal = _signal + _noise;
        }

        private void processToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _processed = Operation.SpectralSubtract(_signal + _noise, _noise);

            processedPlot.Signal = _processed;
        }

        private async void playSignalButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_signal, _bitDepth);
        }

        private async void playNoiseButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_signal + _noise, _bitDepth);
        }

        private async void playProcessedButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_processed, _bitDepth);
        }
    }
}
