using System;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Interfaces;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.DemoForms
{
    public partial class OnlineDemoForm : Form
    {
        DiscreteSignal _signal;
        DiscreteSignal _filtered;

        BlockConvolver _blockConvolver;

        /// <summary>
        /// Buffer for input chunk
        /// </summary>
        float[] _input;

        /// <summary>
        /// Buffer for output chunk
        /// </summary>
        float[] _output;

        Random _randomizer = new Random();

        int _offset = 0;
        int _filteredOffset = 0;
        int _chunkNo = 0;
        int _fftSize;

        public OnlineDemoForm()
        {
            InitializeComponent();
            ApplySettings();

            signalPlot.Stride = 100;
            filteredSignalPlot.Stride = 100;
            filteredFullSignalPlot.Stride = 1000;
        }

        private void ApplySettings()
        {
            _fftSize = int.Parse(fftSizeTextBox.Text);

            chunkTimer.Interval = int.Parse(intervalTextBox.Text);
            var filter = DesignFilter.FirLp(int.Parse(kernelSizeTextBox.Text), 0.2);
            _blockConvolver = BlockConvolver.FromFilter(filter, _fftSize);

            _output = new float[_blockConvolver.HopSize * 5];
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

            // this signal will accumulate the output chunks in lower panel and it's just for visualization
            _filtered = new DiscreteSignal(_signal.SamplingRate, Math.Min(_signal.Length + _fftSize, 60 * 16000));

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
            _offset = 0;
            _filteredOffset = 0;
            _chunkNo = 0;

            _filtered = new DiscreteSignal(_signal.SamplingRate, Math.Min(_signal.Length + _fftSize, 60 * 16000));
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            ApplySettings();
        }

        /// <summary>
        /// Here is the main function for online processing of chunks:
        /// </summary>
        private void ProcessNewChunk(object sender, EventArgs e)
        {
            // =============================== take next random chunk ====================================

            var randomSize = _randomizer.Next(_blockConvolver.HopSize / 5 + 1, _blockConvolver.HopSize * 4);

            var currentChunkSize = Math.Min(randomSize,
                                            Math.Min(_filtered.Length - _filteredOffset - _fftSize,
                                                     _signal.Length - _offset));

            _input = _signal[_offset, _offset + currentChunkSize].Samples;

            // ===========================================================================================



            // ===================================== process it ==========================================

            int readyCount = _blockConvolver.ProcessChunks(_input, _output);  // process everything that's available

            if (readyCount > 0)                                               // if new output is ready
            {
                // do what we need with the output block, e.g. :
                _output.FastCopyTo(_filtered.Samples, readyCount, 0, _filteredOffset);

                // track the offset
                _offset += readyCount;
                _filteredOffset += readyCount;
            }



            // ================================= visualize signals =======================================

            signalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _input);
            if (readyCount > 0)
            {
                filteredSignalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _output).First(readyCount);
            }

            if (_filteredOffset >= _filtered.Length - readyCount)
            {
                _filteredOffset = 0;
                _filtered = new DiscreteSignal(_signal.SamplingRate, Math.Min(_signal.Length + _fftSize, 60 * 16000));
            }
            filteredFullSignalPlot.Signal = _filtered;



            // ================================== not important stuff =====================================

            if (_offset > _signal.Length)
            {
                _offset = 0;              // start all over again
                _filteredOffset = 0;
                _chunkNo = 0;
                _blockConvolver.Reset();
            }

            _chunkNo++;

            labelInfo.Text = $"Chunk #{_chunkNo + 1} / Processed {(float)_offset/_signal.SamplingRate} seconds";
        }
    }
}
