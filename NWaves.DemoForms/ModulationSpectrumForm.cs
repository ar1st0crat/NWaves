using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Windows;
using SciColorMaps;

namespace NWaves.DemoForms
{
    public partial class ModulationSpectrumForm : Form
    {
        private DiscreteSignal _signal;
        private MsExtractor _extractor;

        private List<FeatureVector> _features;
        private int _featIndex;

        private double[][] _filterbank;

        public ModulationSpectrumForm()
        {
            InitializeComponent();

            var filterCount = int.Parse(filterCountTextBox.Text);
            band1ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band2ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band3ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band4ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band1ComboBox.Text = "1";
            band2ComboBox.Text = "2";
            band3ComboBox.Text = "3";
            band4ComboBox.Text = "4";
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
        }
        
        private void filterbankButton_Click(object sender, EventArgs e)
        {
            var filterCount = int.Parse(filterCountTextBox.Text);
            var samplingRate = int.Parse(samplingRateTextBox.Text);
            var fftSize = int.Parse(fftSizeTextBox.Text);
            var lowFreq = double.Parse(lowFreqTextBox.Text);
            var highFreq = double.Parse(highFreqTextBox.Text);

            int scaleCoeff = 1;
            Tuple<double, double, double>[] bands;

            switch (filterbankComboBox.Text)
            {
                case "Mel":
                    bands = FilterBanks.MelBands(filterCount, fftSize, samplingRate, lowFreq, highFreq, overlapCheckBox.Checked);
                    break;
                case "Bark":
                    bands = FilterBanks.BarkBands(filterCount, fftSize, samplingRate, lowFreq, highFreq, overlapCheckBox.Checked);
                    break;
                case "Critical bands":
                    bands = FilterBanks.CriticalBands(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                case "ERB":
                    bands = null;
                    _filterbank = FilterBanks.Erb(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    
                    // normalization coefficient (for plotting)
                    scaleCoeff = (int)(1.0 / _filterbank.Max(f => f.Max()));

                    // ====================================================
                    // ===================  ! SQUARE ! ====================

                    //foreach (var filter in _filterbank)
                    //{
                    //    for (var j = 0; j < filter.Length; j++)
                    //    {
                    //        var squared = filter[j] * filter[j];
                    //        filter[j] = squared;
                    //    }
                    //}

                    break;
                default:
                    bands = FilterBanks.HerzBands(filterCount, fftSize, samplingRate, lowFreq, highFreq, overlapCheckBox.Checked);
                    break;
            }

            if (bands != null)
            {
                switch (shapeComboBox.Text)
                {
                    case "Triangular":
                        _filterbank = FilterBanks.Triangular(fftSize, samplingRate, bands);
                        break;
                    case "Trapezoidal":
                        _filterbank = FilterBanks.Trapezoidal(fftSize, samplingRate, bands);
                        break;
                    case "BiQuad":
                        _filterbank = FilterBanks.BiQuad(fftSize, samplingRate, bands);
                        break;
                    default:
                        _filterbank = FilterBanks.Rectangular(fftSize, samplingRate, bands);
                        break;
                }
            }

            band1ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band2ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band3ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band4ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band1ComboBox.Text = "1";
            band2ComboBox.Text = "2";
            band3ComboBox.Text = "3";
            band4ComboBox.Text = "4";

            DrawFilterbank(_filterbank, scaleCoeff);
        }

        private void computeButton_Click(object sender, EventArgs e)
        {
            var windowSize = double.Parse(analysisFftTextBox.Text);
            var overlapSize = double.Parse(hopSizeTextBox.Text);
            var modulationFftSize = int.Parse(longTermFftSizeTextBox.Text);
            var modulationHopSize = int.Parse(longTermHopSizeTextBox.Text);

            // ===== test modulation spectrum for Mfcc features =====
            //
            //var mfccExtractor = new MfccExtractor(13, _signal.SamplingRate,
            //                                          windowSize: windowSize,
            //                                          overlapSize: overlapSize);
            //var vectors = mfccExtractor.ComputeFrom(_signal);
            //FeaturePostProcessing.NormalizeMean(vectors);

            //_extractor = new MsExtractor(_signal.SamplingRate,
            //                             windowSize, overlapSize,
            //                             modulationFftSize, modulationHopSize,
            //                             featuregram: vectors.Select(v => v.Features));

            _extractor = new MsExtractor(_signal.SamplingRate,
                                         windowSize, overlapSize,
                                         modulationFftSize, modulationHopSize,
                                         filterbank: _filterbank, window: WindowTypes.Hamming);
            _features = _extractor.ComputeFrom(_signal);
            _featIndex = 0;

            infoLabel.Text = $"{_features.Count}x{_features[0].Features.Length}";

            DrawEnvelopes(_extractor.Envelopes);
            DrawModulationSpectrum(_extractor.MakeSpectrum2D(_features[_featIndex]));
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (_featIndex < _features.Count - 1) _featIndex++;
            DrawModulationSpectrum(_extractor.MakeSpectrum2D(_features[_featIndex]));
        }
        
        private void temporalCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (temporalCheckBox.Checked)
            {
                DrawModulationSpectraHerz(_extractor.VectorsAtHerz(_features, double.Parse(herzTextBox.Text)));
            }
            else
            {
                DrawModulationSpectrum(_extractor.MakeSpectrum2D(_features[_featIndex]));
            }
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            if (_featIndex > 0) _featIndex--;
            DrawModulationSpectrum(_extractor.MakeSpectrum2D(_features[_featIndex]));
        }

        private void bandComboBox_TextChanged(object sender, EventArgs e)
        {
            if (_extractor == null)
            {
                return;
            }

            DrawEnvelopes(_extractor.Envelopes);
        }

        #region drawing

        private void DrawFilterbank(double[][] filterbank, int scaleCoeff = 1)
        {
            var g = filterbankPanel.CreateGraphics();
            g.Clear(Color.White);

            var rand = new Random();

            var offset = filterbankPanel.Height - 20;
            scaleCoeff *= 100/*px*/;
          
            for (var j = 0; j < filterbank.Length; j++)
            {
                var pen = new Pen(Color.FromArgb(rand.Next() % 255, rand.Next() % 255, rand.Next() % 255));

                var i = 1;
                var x = 2;

                while (i < filterbank[j].Length)
                {
                    g.DrawLine(pen,
                        x - 2, (float)-filterbank[j][i - 1] * scaleCoeff + offset,
                        x, (float)-filterbank[j][i] * scaleCoeff + offset);
                    x += 2;
                    i++;
                }

                pen.Dispose();
            }
        }

        private void DrawEnvelopes(double[][] envelopes)
        {
            var g = envelopesPanel.CreateGraphics();
            g.Clear(Color.White);

            var xOffset = 10;
            var offsets = Enumerable.Range(0, 4).Select(i => 80 + i * 80).ToArray();

            var blackPen = new Pen(Color.Black);
            var pen = new Pen(Color.Blue);

            var envNo = new []
            {
                int.Parse(band1ComboBox.Text),
                int.Parse(band2ComboBox.Text),
                int.Parse(band3ComboBox.Text),
                int.Parse(band4ComboBox.Text)
            };

            var stride = 2;
            for (var i = 0; i < 4; i++)
            {
                var en = envNo[i] - 1;

                g.DrawLine(blackPen, xOffset, offsets[i], envelopesPanel.Width - xOffset, offsets[i]);
                g.DrawLine(blackPen, xOffset, offsets[i] - 70, xOffset, offsets[i]);

                var x = stride;
                for (var j = 1; j < envelopes[en].Length; j++)
                {
                    g.DrawLine(pen,
                        xOffset + x - stride, (float)-envelopes[en][j - 1] * 1.5f + offsets[i],
                        xOffset + x,     (float)-envelopes[en][j] * 1.5f + offsets[i]);
                    x += stride;
                }
            }

            pen.Dispose();
        }

        private void DrawModulationSpectrum(double[][] spectrum)
        {
            var minValue = spectrum.SelectMany(s => s).Min();
            var maxValue = spectrum.SelectMany(s => s).Max();
            
            var cmap = new MirrorColorMap(new ColorMap("bone", minValue, maxValue));

            var g = modulationSpectrumPanel.CreateGraphics();
            g.Clear(Color.White);

            var spectrumBitmap = new Bitmap(spectrum[0].Length, spectrum.Length);

            for (var i = 0; i < spectrum.Length; i++)
            {
                for (var j = 0; j < spectrum[i].Length; j++)
                {
                    spectrumBitmap.SetPixel(j, spectrum.Length - 1 - i, cmap.GetColor(spectrum[i][j]));
                }
            }

            g.DrawImage(spectrumBitmap, 25, 25, modulationSpectrumPanel.Width - 25, modulationSpectrumPanel.Height - 25);
        }

        private void DrawModulationSpectraHerz(List<double[]> spectra)
        {
            var minValue = spectra.SelectMany(s => s).Min();
            var maxValue = spectra.SelectMany(s => s).Max();

            var cmap = new ColorMap("blues", minValue, maxValue);

            var g = modulationSpectrumPanel.CreateGraphics();
            g.Clear(Color.White);

            var spectrumBitmap = new Bitmap(spectra.Count, spectra[0].Length);

            for (var i = 0; i < spectra.Count; i++)
            {
                for (var j = 0; j < spectra[i].Length; j++)
                {
                    spectrumBitmap.SetPixel(i, spectra[i].Length - 1 - j, cmap.GetColor(spectra[i][j]));
                }
            }

            g.DrawImage(spectrumBitmap, 25, 25, modulationSpectrumPanel.Width - 25, modulationSpectrumPanel.Height - 25);
        }

        #endregion
    }
}
