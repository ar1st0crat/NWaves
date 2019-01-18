using System;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Interfaces;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.DemoForms
{
    public partial class OnlineDemoForm : Form
    {
        DiscreteSignal _signal;
        BlockConvolver _blockConvolver;

        int _chunkPos = 0;

        float[] _input;
        float[] _output;

        public OnlineDemoForm()
        {
            InitializeComponent();
            ApplySettings();
        }

        private void ApplySettings()
        {
            var fftSize = int.Parse(fftSizeTextBox.Text);

            chunkTimer.Interval = int.Parse(intervalTextBox.Text);
            var filter = DesignFilter.FirLp(int.Parse(kernelSizeTextBox.Text), 0.25);
            _blockConvolver = BlockConvolver.FromFilter(filter, fftSize);

            _input = new float[fftSize];
            _output = new float[fftSize];
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var stream = new FileStream(ofd.FileName, FileMode.Open))
            {
                IAudioContainer waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Left];
            }

            signalPlot.Signal = _signal;

            Text = $"{ofd.FileName} | {_signal.Length} samples | {_signal.Duration} seconds";
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            ProcessNewChunk(this, null);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            chunkTimer.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            chunkTimer.Stop();
            _chunkPos = 0;
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ApplySettings();
        }

        private void ProcessNewChunk(object sender, EventArgs e)
        {
            // take next chunk

            var length = Math.Min(_input.Length, _signal.Length - _chunkPos);
            _signal.Samples.FastCopyTo(_input, length, _chunkPos);
            _chunkPos += _blockConvolver.HopSize;

            // process it

            _blockConvolver.Process(_input, _output, method: FilteringMethod.OverlapAdd);

            signalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _input);
            filteredSignalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _output);

            if (_chunkPos > _signal.Length)
            {
                _chunkPos = 0;  // start all over again
            }
        }
    }
}
