using NWaves.Audio;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using System;
using System.IO;
using System.Windows.Forms;

namespace NWaves.DemoForms
{
    public partial class HpssForm : Form
    {
        private DiscreteSignal _signal;
        private DiscreteSignal _harmonicSignal;
        private DiscreteSignal _percussiveSignal;

        private short _bitDepth;

        private readonly MemoryStreamPlayer _player = new MemoryStreamPlayer();

        public HpssForm()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
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

            Evaluate();
        }

        private void evaluateButton_Click(object sender, EventArgs e)
        {
            Evaluate();
        }

        private void Evaluate()
        {
            var fftSize = int.Parse(fftSizeTextBox.Text);
            var hopSize = int.Parse(hopSizeTextBox.Text);
            var harmWinSize = int.Parse(harmonicWindowTextBox.Text);
            var percWinSize = int.Parse(percussiveWindowTextBox.Text);
            var masking = HpsMasking.Binary;

            if (maskingComboBox.SelectedIndex == 1) masking = HpsMasking.WienerOrder1;
            else if (maskingComboBox.SelectedIndex == 2) masking = HpsMasking.WienerOrder2;

            var hpss = new HarmonicPercussiveSeparator(fftSize, hopSize, harmWinSize, percWinSize, masking)
                                .EvaluateSignals(_signal);
                                //.EvaluateSpectrograms(_signal);

            var stft = new Stft(512, 256);

            _harmonicSignal = hpss.Item1;// new DiscreteSignal(_signal.SamplingRate, stft.ReconstructMagnitudePhase(hpss.Item1));
            _percussiveSignal = hpss.Item2;// new DiscreteSignal(_signal.SamplingRate, stft.ReconstructMagnitudePhase(hpss.Item2));

            spectrogramPlot1.Spectrogram = stft.Spectrogram(_signal);
            spectrogramPlot2.Spectrogram = stft.Spectrogram(_harmonicSignal);
            spectrogramPlot3.Spectrogram = stft.Spectrogram(_percussiveSignal);
        }

        private async void playButton1_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_signal);
        }

        private async void playButton2_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_harmonicSignal);
        }

        private async void playButton3_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_percussiveSignal * 2);
        }
    }
}
