using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Mci;
using NWaves.Effects;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using SciColorMaps;

namespace NWaves.DemoForms
{
    public partial class EffectsForm : Form
    {
        private DiscreteSignal _signal;
        private List<double[]> _spectrogram;
        
        private DiscreteSignal _filteredSignal;
        private List<double[]> _filteredSpectrogram;

        private readonly Stft _stft = new Stft(256, fftSize: 256);

        private string _waveFileName;

        private readonly MciAudioPlayer _player = new MciAudioPlayer();


        public EffectsForm()
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
                _signal = waveFile[Channels.Left];

                var truncLength = _signal.SamplingRate * 5;
                if (truncLength < _signal.Length)
                {
                    _signal = _signal.First(truncLength);
                }
            }

            DrawSignal(signalBeforeFilteringPanel, _signal);

            _spectrogram = _stft.Spectrogram(_signal.Samples);
            DrawSpectrogram(spectrogramBeforeFilteringPanel, _spectrogram);
        }

        private void applyEffectButton_Click(object sender, EventArgs e)
        {
            IFilter effect;

            if (tremoloRadioButton.Checked)
            {
                var freq = double.Parse(tremoloFrequencyTextBox.Text);
                var index = double.Parse(tremoloIndexTextBox.Text);
                effect = new TremoloEffect(freq, index);
            }
            else if (overdriveRadioButton.Checked)
            {
                effect = new OverdriveEffect();
            }
            else if (distortionRadioButton.Checked)
            {
                var gain = double.Parse(distortionGainTextBox.Text);
                var mix = double.Parse(distortionMixTextBox.Text);
                effect = new DistortionEffect(gain, mix);
            }
            else if (tubeDistortionRadioButton.Checked)
            {
                var gain = double.Parse(distortionGainTextBox.Text);
                var mix = double.Parse(distortionMixTextBox.Text);
                var dist = double.Parse(distTextBox.Text);
                var q = double.Parse(qTextBox.Text);
                effect = new TubeDistortionEffect(gain, mix, q, dist);
            }
            else if (echoRadioButton.Checked)
            {
                var delay = double.Parse(echoDelayTextBox.Text);
                var decay = double.Parse(echoDecayTextBox.Text);
                effect = new EchoEffect(delay, decay);
            }
            else if (wahwahRadioButton.Checked)
            {
                var lfoFrequency = double.Parse(lfoFreqTextBox.Text);
                var minFrequency = double.Parse(minFreqTextBox.Text);
                var maxFrequency = double.Parse(maxFreqTextBox.Text);
                var q = double.Parse(lfoQTextBox.Text);
                effect = new WahwahEffect(lfoFrequency, minFrequency, maxFrequency, q);
            }
            else
            {
                var lfoFrequency = double.Parse(lfoFreqTextBox.Text);
                var minFrequency = double.Parse(minFreqTextBox.Text);
                var maxFrequency = double.Parse(maxFreqTextBox.Text);
                var q = double.Parse(lfoQTextBox.Text);
                effect = new PhaserEffect(lfoFrequency, minFrequency, maxFrequency, q);
            }

            _filteredSignal = effect.ApplyTo(_signal, FilteringOptions.Auto);

            DrawSignal(signalAfterFilteringPanel, _signal);

            _filteredSpectrogram = _stft.Spectrogram(_filteredSignal.Samples);
            DrawSpectrogram(spectrogramAfterFilteringPanel, _filteredSpectrogram);
        }

        #region drawing

        private void DrawSignal(Control panel, DiscreteSignal signal, int stride = 256)
        {
            var g = panel.CreateGraphics();
            g.Clear(Color.White);

            var offset = panel.Height / 2;

            var pen = panel == signalBeforeFilteringPanel ? new Pen(Color.Blue) : new Pen(Color.Red);

            var i = 0;
            var x = 0;

            while (i < signal.Length - stride)
            {
                var j = 0;
                var min = 0.0;
                var max = 0.0;
                while (j < stride)
                {
                    if (signal[i + j] > max) max = signal[i + j];
                    if (signal[i + j] < min) min = signal[i + j];
                    j++;
                }
                g.DrawLine(pen, x, (float)-min * 70 + offset, x, (float)-max * 70 + offset);
                x++;
                i += stride;

            }

            pen.Dispose();
        }

        private void DrawSpectrogram(Control panel, List<double[]> spectrogram)
        {
            var g = panel.CreateGraphics();
            g.Clear(Color.White);

            var spectraCount = spectrogram.Count;

            var minValue = spectrogram.SelectMany(s => s).Min();
            var maxValue = spectrogram.SelectMany(s => s).Max();

            // post-process spectrogram for better visualization
            for (var i = 0; i < spectraCount; i++)
            {
                spectrogram[i] = spectrogram[i].Select(s =>
                {
                    var sqrt = Math.Sqrt(s);
                    return sqrt*3 < maxValue ? sqrt*3 : sqrt/1.5;
                })
                .ToArray();
            }
            maxValue /= 12;

            var cmap = new ColorMap("magma", minValue, maxValue);


            var spectrogramBitmap = new Bitmap(spectrogram.Count, spectrogram[0].Length);

            for (var i = 0; i < spectrogram.Count; i++)
            {
                for (var j = 0; j < spectrogram[i].Length; j++)
                {
                    spectrogramBitmap.SetPixel(i, spectrogram[i].Length - 1 - j, cmap.GetColor(spectrogram[i][j]));
                }
            }

            g.DrawImage(spectrogramBitmap, 0, 0);
        }

        #endregion

        #region playback

        private async void playSignalButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_waveFileName);
        }

        private async void playFilteredSignalButton_Click(object sender, EventArgs e)
        {
            // create temporary file
            const string tmpFilename = "tmpfiltered.wav";
            using (var stream = new FileStream(tmpFilename, FileMode.Create))
            {
                var waveFile = new WaveFile(_filteredSignal);
                waveFile.SaveTo(stream);
            }

            await _player.PlayAsync(tmpFilename);
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            _player.Stop();
        }

        #endregion
    }
}
