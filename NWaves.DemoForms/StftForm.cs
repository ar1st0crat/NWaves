using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Filters;
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

            _spectrogram = _stft.Spectrogram(_signal);

            var processed = _stft.Inverse(_stft.Direct(_signal));
            _processedSignal = new DiscreteSignal(_signal.SamplingRate, processed);

            
            // 1) check also this:
            //var mp = _stft.MagnitudePhaseSpectrogram(_signal);
            //var processed = _stft.ReconstructMagnitudePhase(mp, false);
            //_processedSignal = new DiscreteSignal(_signal.SamplingRate, processed);

            // 2) or check this:
            //var processed = new GriffinLimReconstructor(_spectrogram, _stft).Reconstruct();
            //_processedSignal = new DiscreteSignal(_signal.SamplingRate, processed);

            signalPanel.Gain = 120;
            signalPanel.Signal = _signal;
            processedSignalPanel.Gain = 120;
            processedSignalPanel.Signal = _processedSignal;

            spectrogramPanel.Spectrogram = _spectrogram;


            //// StftC - has complex FFT

            //// RealFFT-based Stft is 30% faster!

            //var sr = new Stft(2048, 256);
            //var sc = new StftC(2048, 256);

            //var sw = new Stopwatch();

            //sw.Start();

            //for (var i = 0; i < 10; i++)
            //{
            //    var processed1 = sr.Inverse(sr.Direct(_signal));
            //    _processedSignal = new DiscreteSignal(_signal.SamplingRate, processed1);
            //}

            //sw.Stop();

            //var t1 = sw.Elapsed;


            //sw.Reset();
            //sw.Start();

            //for (var i = 0; i < 10; i++)
            //{
            //    var processed1 = sc.Inverse(sc.Direct(_signal));
            //    _processedSignal = new DiscreteSignal(_signal.SamplingRate, processed1);
            //}

            //sw.Stop();

            //var t2 = sw.Elapsed;

            //MessageBox.Show(t1 + " " + t2);
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
