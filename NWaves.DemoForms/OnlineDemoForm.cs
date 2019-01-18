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
        int _chunkNo = 0;

        float[] _input;
        float[] _output;

        int _fftSize;

        public OnlineDemoForm()
        {
            InitializeComponent();
            ApplySettings();
        }

        private void ApplySettings()
        {
            _fftSize = int.Parse(fftSizeTextBox.Text);

            chunkTimer.Interval = int.Parse(intervalTextBox.Text);
            var filter = DesignFilter.FirLp(int.Parse(kernelSizeTextBox.Text), 0.2);
            _blockConvolver = BlockConvolver.FromFilter(filter, _fftSize);

            _input = new float[_fftSize];
            _output = new float[_fftSize];
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
            _chunkNo = 0;
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
            _chunkNo++;

            // process it

            _blockConvolver.Process(_input, _output, method: FilteringMethod.OverlapSave);

            signalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _input);
            filteredSignalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _output);

            if (_chunkPos > _signal.Length)
            {
                _chunkPos = 0;              // start all over again
                _chunkNo = 0;
                _blockConvolver.Reset();
            }

            labelInfo.Text = $"Chunk #{_chunkNo + 1} / Processed {(float)_chunkNo*_fftSize/_signal.SamplingRate} seconds";
        }
    }
}
