using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Interfaces;
using NWaves.Audio.Mci;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class StftForm : Form
    {
        private DiscreteSignal _signal;
        private DiscreteSignal _processedSignal;

        private Stft _stft;
        private List<double[]> _spectrogram;

        private string _waveFileName;

        private readonly IAudioPlayer _player = new MciAudioPlayer();


        public StftForm()
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

            _waveFileName = ofd.FileName;

            using (var stream = new FileStream(_waveFileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Average];
            }

            _stft = new Stft(1024, 256);

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
            const string tmpFilename = "tmpfiltered.wav";
            using (var stream = new FileStream(tmpFilename, FileMode.Create))
            {
                var waveFile = new WaveFile(_processedSignal);
                waveFile.SaveTo(stream);
            }

            await _player.PlayAsync(tmpFilename);
        }
    }
}
