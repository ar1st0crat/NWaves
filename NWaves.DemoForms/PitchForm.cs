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
using SciColorMaps;
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
            
            // obtain spectrogram

            _stft = new Stft(_fftSize, _overlapSize, WindowTypes.Rectangular, _fftSize);
            var spectrogram = _stft.Spectrogram(_signal.Samples);

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

            var cmap = new ColorMap("viridis", minValue, maxValue);



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

            DrawSpectrogram(spectrogram, pitches, cmap);

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

            var spectrum = _fft.PowerSpectrum(block, normalize: false)
                               .Samples
                               .Select(s => LevelScale.ToDecibel(s))
                               .ToArray();

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

            _fft.Direct(real, imag);

            var avg = spectrum.Average();

            var spectrumEstimate = real.Take(_fftSize / 2 + 1)
                                       .Select(s => s * 40 / _fftSize - avg)
                                       .ToArray();

            DrawSpectrum(spectrum, spectrumEstimate, _fftSize / peakIndex);
            DrawCepstrum(cepstrum, peakIndex);
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

            DrawAutoCorrelation(autoCorrelation.Samples, peakIndex);
        }

        #region drawing

        private void DrawSpectrum(double[] spectrum, double[] estimate = null, int peakIndex = 0)
        {
            var g = spectrumPanel.CreateGraphics();
            g.Clear(Color.White);

            var xOffset = 15;
            var yOffset = 15;
            var offset = spectrumPanel.Height / 2;

            var blackPen = new Pen(Color.Black);
            g.DrawLine(blackPen, xOffset, offset, spectrumPanel.Width - xOffset, offset);
            g.DrawLine(blackPen, xOffset, yOffset, xOffset, spectrumPanel.Height - yOffset);
            blackPen.Dispose();

            var pen = new Pen(Color.Blue);
            
            var i = 1;
            var x = xOffset + 1;

            while (i < spectrum.Length)
            {
                if (Math.Abs(spectrum[i]) < spectrumPanel.Height)
                {
                    g.DrawLine(pen, x - 1, (float)-spectrum[i - 1] + offset, x, (float)-spectrum[i] + offset);
                }
                x++;
                i++;
            }

            pen.Dispose();

            var redPen = new Pen(Color.Red, 2);
            g.DrawLine(redPen, peakIndex + xOffset, yOffset, peakIndex + xOffset, spectrumPanel.Height - yOffset);

            if (estimate != null)
            {
                i = 1;
                x = xOffset + 1;

                while (i < estimate.Length)
                {
                    if (Math.Abs(estimate[i]) < spectrumPanel.Height)
                    {
                        g.DrawLine(redPen, x - 1, (float)-estimate[i - 1] + offset, x, (float)-estimate[i] + offset);
                    }
                    x++;
                    i++;
                }
            }

            var info = string.Format("{0:F2} Hz", (double)_signal.SamplingRate * peakIndex / _fftSize);
            g.DrawString(info, new Font("arial", 16), new SolidBrush(Color.Red), 100, 30);

            redPen.Dispose();
        }

        private void DrawCepstrum(double[] cepstrum, int peakIndex = 0)
        {
            var g = cepstrumPanel.CreateGraphics();
            g.Clear(Color.White);

            var xOffset = 15;
            var yOffset = 15;
            var offset = spectrumPanel.Height / 2;

            var blackPen = new Pen(Color.Black);
            g.DrawLine(blackPen, xOffset, offset, spectrumPanel.Width - xOffset, offset);
            g.DrawLine(blackPen, xOffset, yOffset, xOffset, spectrumPanel.Height - yOffset);
            
            blackPen.Dispose();
            
            var pen = new Pen(Color.Blue);

            var i = 2;
            var x = xOffset + 2;

            while (i < cepstrum.Length)
            {
                if (Math.Abs(cepstrum[i] / 5) < spectrumPanel.Height)
                {
                    g.DrawLine(pen, x - 1, (float)-cepstrum[i - 1] / 5 + offset, x, (float)-cepstrum[i] / 5 + offset);
                }
                x++;
                i++;
            }

            pen.Dispose();

            var redPen = new Pen(Color.Red, 2);
            g.DrawLine(redPen, peakIndex + xOffset, yOffset, peakIndex + xOffset, spectrumPanel.Height - yOffset);
            redPen.Dispose();
        }

        private void DrawAutoCorrelation(double[] autocorr, int peakIndex = 0)
        {
            var g = autoCorrPanel.CreateGraphics();
            g.Clear(Color.White);

            var xOffset = 15;
            var yOffset = 15;
            var offset = spectrumPanel.Height / 2;

            var blackPen = new Pen(Color.Black);
            g.DrawLine(blackPen, xOffset, offset, spectrumPanel.Width - xOffset, offset);
            g.DrawLine(blackPen, xOffset, yOffset, xOffset, spectrumPanel.Height - yOffset);

            blackPen.Dispose();

            var pen = new Pen(Color.Blue);

            var i = 1;
            var x = xOffset + 1;

            while (i < autocorr.Length)
            {
                if (Math.Abs(autocorr[i] * 5) < autoCorrPanel.Height)
                {
                    g.DrawLine(pen, x - 1, (float)-autocorr[i - 1] * 5 + offset, x, (float)-autocorr[i] * 5 + offset);
                }
                x++;
                i++;
            }

            pen.Dispose();

            var redPen = new Pen(Color.Red, 2);
            g.DrawLine(redPen, peakIndex + xOffset, yOffset, peakIndex + xOffset, spectrumPanel.Height - yOffset);
            redPen.Dispose();
        }

        private void DrawSpectrogram(List<double[]> spectrogram, List<double> pitches, ColorMap cmap)
        {
            var g = spectrogramPanel.CreateGraphics();
            g.Clear(Color.White);
            
            var spectrogramBitmap = new Bitmap(spectrogram.Count, spectrogram[0].Length);

            for (var i = 0; i < spectrogram.Count; i++)
            {
                for (var j = 0; j < spectrogram[i].Length; j++)
                {
                    spectrogramBitmap.SetPixel(i, j, cmap.GetColor(spectrogram[i][j]));
                }
            }

            g.DrawImage(spectrogramBitmap, 0, 0);

            var pen = new Pen(Color.DeepPink, 7);

            for (var i = 1; i < pitches.Count; i++)
            {
                g.DrawLine(pen, i - 1, (int)(_signal.SamplingRate / pitches[i - 1]),
                                i,     (int)(_signal.SamplingRate / pitches[i]));
            }

            pen.Dispose();
        }

        #endregion
    }
}
