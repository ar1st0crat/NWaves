using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
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
        private int _overlapSize;
        private int _cepstrumSize;

        private int _specNo;

        public PitchForm()
        {
            InitializeComponent();

            _fftSize = 1024;
            _overlapSize = 100;
            _cepstrumSize = 256;

            fftSizeTextBox.Text = _fftSize.ToString();
            overlapSizeTextBox.Text = _overlapSize.ToString();
            cepstrumSizeTextBox.Text = _cepstrumSize.ToString();

            _fft = new Fft(_fftSize);
            _cepstralTransform = new CepstralTransform(_cepstrumSize, _fftSize);

            cepstrumPanel.Gain = 0.2;
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

            var count = _signal.Length / _overlapSize;
            specNoComboBox.DataSource = Enumerable.Range(1, count).ToArray();

            UpdateSpectra();
            UpdateAutoCorrelation();
            
            // track pitch

            var pitches = new List<double>();

            var freqResolution = (double)_signal.SamplingRate / _fftSize;

            var pos = 0;
            while (pos + _fftSize < _signal.Length)
            {
                var ceps = _cepstralTransform.Direct(_signal[pos, pos + _fftSize]);
                
                var pitch1 = (int)(0.0025 * _signal.SamplingRate);     // 2,5 ms = 400Hz
                var pitch2 = (int)(0.0125 * _signal.SamplingRate);     // 12,5 ms = 80Hz

                var max = Math.Abs(ceps[pitch1]);
                var maxIdx = pitch1;
                for (var k = pitch1 + 1; k <= pitch2; k++)
                {
                    if (Math.Abs(ceps[k]) > max)
                    {
                        max = Math.Abs(ceps[k]);
                        maxIdx = k;
                    }
                }

                pitches.Add(freqResolution * maxIdx);

                pos += _overlapSize;
            }


            // obtain spectrogram

            _stft = new Stft(_fftSize, _overlapSize, WindowTypes.Rectangular);
            var spectrogram = _stft.Spectrogram(_signal);

            spectrogramPanel.ColorMapName = "viridis";
            spectrogramPanel.Spectrogram = spectrogram.Select(s => s.Take(224).ToArray()).ToList();
            spectrogramPanel.Markline = pitches.Select(p => _signal.SamplingRate / p).ToArray();
            
            _specNo = 0;
        }

        private void specNoComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _specNo = int.Parse(specNoComboBox.Text) - 1;
            UpdateSpectra();
            UpdateAutoCorrelation();
        }
        
        private void prevButton_Click(object sender, EventArgs e)
        {
            _specNo--;
            specNoComboBox.Text = (_specNo + 1).ToString();
            UpdateSpectra();
            UpdateAutoCorrelation();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            _specNo++;
            specNoComboBox.Text = (_specNo + 1).ToString();
            UpdateSpectra();
            UpdateAutoCorrelation();
        }

        private void UpdateSpectra()
        {
            var fftSize = int.Parse(fftSizeTextBox.Text);
            var cepstrumSize = int.Parse(cepstrumSizeTextBox.Text);

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

            var pos = _overlapSize * _specNo;

            var block = _signal[pos, pos + _fftSize];
            block.ApplyWindow(WindowTypes.Hamming);

            var cepstrum = _cepstralTransform.Direct(block).Samples;


            var pitch1 = (int)(0.0025 * _signal.SamplingRate);     // 2,5 ms = 400Hz
            var pitch2 = (int)(0.0125 * _signal.SamplingRate);     // 12,5 ms = 80Hz

            var max = Math.Abs(cepstrum[pitch1]);
            var peakIndex = pitch1;
            for (var k = pitch1 + 1; k <= pitch2; k++)
            {
                if (Math.Abs(cepstrum[k]) > max)
                {
                    max = Math.Abs(cepstrum[k]);
                    peakIndex = k;
                }
            }

            var real = new double[_fftSize];
            var imag = new double[_fftSize];

            for (var i = 0; i < 32; i++)
            {
                real[i] = cepstrum[i];
            }

            var spectrum = _fft.PowerSpectrum(block, normalize: false).Samples;

            var avg = spectrum.Average(s => LevelScale.ToDecibel(s));

            _fft.Direct(real, imag);

            var spectrumEstimate = real.Take(_fftSize / 2 + 1)
                                       .Select(s => s * 40 / _fftSize - avg)
                                       .ToArray();

            spectrumPanel.Line = spectrum;
            spectrumPanel.ToDecibel();
            spectrumPanel.Markline = spectrumEstimate;
            spectrumPanel.Mark = _fftSize / peakIndex;
            spectrumPanel.Legend = string.Format("{0:F2} Hz", (double)_signal.SamplingRate / peakIndex);

            cepstrumPanel.Line = cepstrum;
            cepstrumPanel.Mark = peakIndex;
        }

        private void UpdateAutoCorrelation()
        {
            var pos = _overlapSize * _specNo;

            var block = _signal[pos, pos + _fftSize];
            
            var autoCorrelation = Operation.CrossCorrelate(block, block).Last(_fftSize);

            var pitch1 = (int)(0.0025 * _signal.SamplingRate);     // 2,5 ms = 400Hz
            var pitch2 = (int)(0.0125 * _signal.SamplingRate);     // 12,5 ms = 80Hz

            var max = autoCorrelation[pitch1];
            var peakIndex = pitch1;
            for (var k = pitch1 + 1; k <= pitch2; k++)
            {
                if (autoCorrelation[k] > max)
                {
                    max = autoCorrelation[k];
                    peakIndex = k;
                }
            }

            autoCorrPanel.Line = autoCorrelation.Samples;
            autoCorrPanel.Mark = peakIndex;
        }
    }
}
