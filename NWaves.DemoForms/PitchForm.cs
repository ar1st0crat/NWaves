using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors.Base;
using NWaves.Features;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Windows;
using LevelScale = NWaves.Utils.Scale;

namespace NWaves.DemoForms
{
    public partial class PitchForm : Form
    {
        private DiscreteSignal _signal;

        private Fft _fft;
        private CepstralTransform _cepstralTransform;
        private Stft _stft;

        private int _fftSize;
        private int _hopSize;
        private int _cepstrumSize;

        private List<FeatureVector> _pitches;

        private int _specNo;

        public PitchForm()
        {
            InitializeComponent();

            cepstrumPanel.Gain = 0.2f;
            cepstrumPanel.Stride = 1;
            cepstrumPanel.ForeColor = Color.Blue;
            autoCorrPanel.Gain = 5;
            autoCorrPanel.Stride = 1;
            autoCorrPanel.ForeColor = Color.SeaGreen;
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
                _signal = waveFile[Channels.Left];
            }

            _fftSize = int.Parse(fftSizeTextBox.Text);
            _cepstrumSize = int.Parse(cepstrumSizeTextBox.Text);
            _hopSize = int.Parse(hopSizeTextBox.Text);

            _fft = new Fft(_fftSize);
            _cepstralTransform = new CepstralTransform(_cepstrumSize, _fftSize);

            var pitchExtractor = new PitchExtractor(_signal.SamplingRate,
                                                    (float)_fftSize / _signal.SamplingRate,
                                                    (float)_hopSize / _signal.SamplingRate);

            _pitches = pitchExtractor.ParallelComputeFrom(_signal);

            UpdateSpectrumAndCepstrum();
            UpdateAutoCorrelation();
            
            // obtain spectrogram

            _stft = new Stft(_fftSize, _hopSize, WindowTypes.Rectangular);
            var spectrogram = _stft.Spectrogram(_signal);

            spectrogramPanel.ColorMapName = "viridis";
            spectrogramPanel.Spectrogram = spectrogram.Select(s => s.Take(224).ToArray()).ToList();
            spectrogramPanel.Markline = _pitches.Select(p => p.Features[0] * _fftSize / _signal.SamplingRate).ToArray();

            specNoComboBox.DataSource = Enumerable.Range(1, _pitches.Count).ToArray();

            _specNo = 0;
        }

        private void specNoComboBox_TextChanged(object sender, EventArgs e)
        {
            _specNo = int.Parse(specNoComboBox.Text) - 1;
            UpdateAutoCorrelation();
            UpdateSpectrumAndCepstrum();
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            _specNo--;
            specNoComboBox.Text = (_specNo + 1).ToString();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            _specNo++;
            specNoComboBox.Text = (_specNo + 1).ToString();
        }

        private void UpdateSpectrumAndCepstrum()
        {
            var fftSize = int.Parse(fftSizeTextBox.Text);
            var cepstrumSize = int.Parse(cepstrumSizeTextBox.Text);
            _hopSize = int.Parse(hopSizeTextBox.Text);

            if (fftSize != _fftSize)
            {
                _fftSize = fftSize;
                _fft = new Fft(fftSize);
                _cepstralTransform = new CepstralTransform(cepstrumSize, _fftSize);
            }

            if (cepstrumSize != _cepstrumSize)
            {
                _cepstrumSize = cepstrumSize;
                _cepstralTransform = new CepstralTransform(_cepstrumSize, _fftSize);
            }
            
            var pos = _hopSize * _specNo;
            var block = _signal[pos, pos + _fftSize];
            
            //block.ApplyWindow(WindowTypes.Hamming);

            var cepstrum = _cepstralTransform.Direct(block);

            var pitch = Pitch.FromCepstrum(block);

            // ************************************************************************
            //      just visualize spectrum estimated from cepstral coefficients:
            // ************************************************************************

            var real = new float[_fftSize];
            var imag = new float[_fftSize];

            for (var i = 0; i < 32; i++)
            {
                real[i] = cepstrum[i];
            }

            _fft.Direct(real, imag);

            var spectrum = _fft.PowerSpectrum(block, normalize: false).Samples;
            var avg = spectrum.Average(s => LevelScale.ToDecibel(s));

            var spectrumEstimate = real.Take(_fftSize / 2 + 1)
                                       .Select(s => (float)LevelScale.FromDecibel(s * 40 / _fftSize - avg))
                                       .ToArray();

            spectrumPanel.Line = spectrum;
            spectrumPanel.Markline = spectrumEstimate;
            spectrumPanel.ToDecibel();

            cepstrumPanel.Line = cepstrum.Samples;
            cepstrumPanel.Mark = (int)(_signal.SamplingRate / pitch);
        }

        private void UpdateAutoCorrelation()
        {
            var pos = _hopSize * _specNo;
            var block = _signal[pos, pos + _fftSize];

            var pitch = Pitch.FromAutoCorrelation(block);//.FromZeroCrossingsSchmitt(block);

            spectrumPanel.Mark = (int)(_fftSize * pitch / _signal.SamplingRate);    // pitch index
            spectrumPanel.Legend = string.Format("{0:F2} Hz", pitch);

            var autoCorrelation = Operation.CrossCorrelate(block, block).Last(_fftSize);

            autoCorrPanel.Line = autoCorrelation.Samples;
            autoCorrPanel.Mark = pitch == 0 ? 0 : (int)(_signal.SamplingRate / pitch);  // _pitches[_specNo].Features[0]);         // pitch index
        }
    }
}
