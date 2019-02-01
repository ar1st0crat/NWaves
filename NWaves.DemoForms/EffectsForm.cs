using System;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Effects;
using NWaves.Filters.Base;
using NWaves.Operations;
using NWaves.Operations.Tsm;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class EffectsForm : Form
    {
        private DiscreteSignal _signal;
        private DiscreteSignal _filteredSignal;

        private readonly Stft _stft = new Stft(256, fftSize: 256);

        private string _waveFileName;
        private short _bitDepth;

        private readonly MemoryStreamPlayer _player = new MemoryStreamPlayer();


        public EffectsForm()
        {
            InitializeComponent();

            signalBeforeFilteringPanel.Gain = 80;
            signalAfterFilteringPanel.Gain = 80;
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

            signalBeforeFilteringPanel.Signal = _signal;
            spectrogramBeforeFilteringPanel.Spectrogram = _stft.Spectrogram(_signal);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var stream = new FileStream(sfd.FileName, FileMode.Create))
            {
                var waveFile = new WaveFile(_filteredSignal, _bitDepth);
                waveFile.SaveTo(stream);
            }
        }

        private void applyEffectButton_Click(object sender, EventArgs e)
        {
            IFilter effect;

            var winSize = int.Parse(winSizeTextBox.Text);
            var hopSize = int.Parse(hopSizeTextBox.Text);
            var tsm = (TsmAlgorithm)tsmComboBox.SelectedIndex;

            var shift = float.Parse(pitchShiftTextBox.Text);

            if (tremoloRadioButton.Checked)
            {
                var freq = float.Parse(tremoloFrequencyTextBox.Text);
                var index = float.Parse(tremoloIndexTextBox.Text);
                effect = new TremoloEffect(freq, index);
            }
            else if (overdriveRadioButton.Checked)
            {
                effect = new OverdriveEffect();
            }
            else if (distortionRadioButton.Checked)
            {
                var gain = float.Parse(distortionGainTextBox.Text);
                var mix = float.Parse(distortionMixTextBox.Text);
                effect = new DistortionEffect(gain, mix);
            }
            else if (tubeDistortionRadioButton.Checked)
            {
                var gain = float.Parse(distortionGainTextBox.Text);
                var mix = float.Parse(distortionMixTextBox.Text);
                var dist = float.Parse(distTextBox.Text);
                var q = float.Parse(qTextBox.Text);
                effect = new TubeDistortionEffect(gain, mix, q, dist);
            }
            else if (echoRadioButton.Checked)
            {
                var delay = float.Parse(echoDelayTextBox.Text);
                var decay = float.Parse(echoDecayTextBox.Text);
                effect = new EchoEffect(delay, decay);
            }
            else if (delayRadioButton.Checked)
            {
                var delay = float.Parse(echoDelayTextBox.Text);
                var decay = float.Parse(echoDecayTextBox.Text);
                effect = new DelayEffect(delay, decay);
            }
            else if (wahwahRadioButton.Checked)
            {
                var lfoFrequency = float.Parse(lfoFreqTextBox.Text);
                var minFrequency = float.Parse(minFreqTextBox.Text);
                var maxFrequency = float.Parse(maxFreqTextBox.Text);
                var q = float.Parse(lfoQTextBox.Text);
                effect = new WahwahEffect(lfoFrequency, minFrequency, maxFrequency, q);
            }
            else if (pitchShiftRadioButton.Checked)
            {
                effect = pitchShiftCheckBox.Checked ? new PitchShiftEffect(shift, winSize, hopSize, tsm) : null;
            }
            else
            {
                var lfoFrequency = float.Parse(lfoFreqTextBox.Text);
                var minFrequency = float.Parse(minFreqTextBox.Text);
                var maxFrequency = float.Parse(maxFreqTextBox.Text);
                var q = float.Parse(lfoQTextBox.Text);
                effect = new PhaserEffect(lfoFrequency, minFrequency, maxFrequency, q);
            }

            _filteredSignal = effect != null ?
                              effect.ApplyTo(_signal, FilteringMethod.Auto) :
                              Operation.TimeStretch(_signal, shift, tsm);
                              //Operation.TimeStretch(_signal, shift, winSize, hopSize, tsm);

            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal.Samples);
        }

        #region playback

        private async void playSignalButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_waveFileName);
        }

        private async void playFilteredSignalButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_filteredSignal, _bitDepth);
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _player.Stop();
        }

        #endregion
    }
}
