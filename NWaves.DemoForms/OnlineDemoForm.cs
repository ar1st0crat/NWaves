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

        OlaBlockConvolver _blockConvolver;

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
        int _filteredLength = 0;
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
            _blockConvolver = OlaBlockConvolver.FromFilter(filter, _fftSize);

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

            _filteredLength = Math.Min(_signal.Length + _fftSize, 60 * 16000);
            _filtered = new DiscreteSignal(_signal.SamplingRate, _filteredLength);

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

            _filtered = new DiscreteSignal(_signal.SamplingRate, _filteredLength);
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
            // =========================== take next chunk of random size ================================
            //                           (random size is chosen carefully)

            var randomSize = Math.Min(_randomizer.Next(_blockConvolver.HopSize / 4, _blockConvolver.HopSize * 4),
                             Math.Min(_signal.Length - _offset,
                                      _filteredLength - _filteredOffset));

            _input = _signal[_offset, _offset + randomSize].Samples;
            

            // ===================================== process it ==========================================

            _blockConvolver.Process(_input, _output);

            
            // ===================== do what we want with a new portion of data ==========================

            _output.FastCopyTo(_filtered.Samples, _input.Length, 0, _filteredOffset);

            _offset += _input.Length;
            _filteredOffset += _input.Length;

            // ================================= visualize signals =======================================

            signalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _input);
            filteredSignalPlot.Signal = new DiscreteSignal(_signal.SamplingRate, _output);

            if (_filteredOffset >= _filtered.Length - _input.Length)
            {
                _filteredOffset = 0;
                _filtered = new DiscreteSignal(_signal.SamplingRate, Math.Min(_signal.Length + _fftSize, 60 * 16000));
            }
            filteredFullSignalPlot.Signal = _filtered;

            // ====================== reset if we've reached the end of a signal =========================

            if (_offset + randomSize >= _signal.Length)
            {
                _offset = 0;
                _filteredOffset = 0;
                _chunkNo = 0;
                _blockConvolver.Reset();

                _filtered = new DiscreteSignal(_signal.SamplingRate, _filteredLength);
            }

            _chunkNo++;

            labelInfo.Text = $"Chunk #{_chunkNo + 1} / Processed {(float)_offset/_signal.SamplingRate} seconds";
        }
    }
}
