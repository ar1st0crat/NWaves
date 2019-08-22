using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Options;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Windows;
using SciColorMaps;

namespace NWaves.DemoForms
{
    public partial class AmsForm : Form
    {
        private DiscreteSignal _signal;
        private AmsExtractor _extractor;

        private List<float[]> _features;
        private int _featIndex;

        private float[][] _filterbank;

        public AmsForm()
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
            var lowFreq = float.Parse(lowFreqTextBox.Text);
            var highFreq = float.Parse(highFreqTextBox.Text);

            (double, double, double)[] bands;

            switch (filterbankComboBox.Text)
            {
                case "Mel":
                    bands = FilterBanks.MelBands(filterCount, samplingRate, lowFreq, highFreq, overlapCheckBox.Checked);
                    break;
                case "Bark":
                    bands = FilterBanks.BarkBands(filterCount, samplingRate, lowFreq, highFreq, overlapCheckBox.Checked);
                    break;
                case "Critical bands":
                    bands = FilterBanks.CriticalBands(filterCount, samplingRate, lowFreq, highFreq);
                    break;
                case "Octave bands":
                    bands = FilterBanks.OctaveBands(filterCount, samplingRate, lowFreq, highFreq, overlapCheckBox.Checked);
                    break;
                case "ERB":
                    bands = null;
                    _filterbank = FilterBanks.Erb(filterCount, fftSize, samplingRate, lowFreq, highFreq);

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

                    // normalization coefficient (for plotting)
                    var scaleCoeff = (int)(1.0 / _filterbank.Max(f => f.Max()));
                    filterbankPanel.Gain = 100 * scaleCoeff;


                    break;
                default:
                    bands = FilterBanks.HerzBands(filterCount, samplingRate, lowFreq, highFreq, overlapCheckBox.Checked);
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

            filterbankPanel.Groups = _filterbank;
        }

        private void computeButton_Click(object sender, EventArgs e)
        {
            var frameDuration = double.Parse(analysisFftTextBox.Text);
            var hopDuration = double.Parse(hopSizeTextBox.Text);
            var modulationFftSize = int.Parse(longTermFftSizeTextBox.Text);
            var modulationHopSize = int.Parse(longTermHopSizeTextBox.Text);

            // ===== test modulation spectrum for Mfcc features =====
            //
            //var mfccExtractor = new MfccExtractor(
            //    new MfccOptions
            //    {
            //        SamplingRate = _signal.SamplingRate,
            //        FeatureCount = 13,
            //        FrameDuration = frameDuration,
            //        HopDuration = hopDuration
            //    });
            //var vectors = mfccExtractor.ComputeFrom(_signal);
            ////FeaturePostProcessing.NormalizeMean(vectors);

            //var options = new AmsOptions
            //{
            //    SamplingRate = _signal.SamplingRate,
            //    FrameDuration = frameDuration,
            //    HopDuration = hopDuration,
            //    ModulationFftSize = modulationFftSize,
            //    ModulationHopSize = modulationHopSize,
            //    Featuregram = vectors
            //};
            //_extractor = new AmsExtractor(options);


            var options = new AmsOptions
            {
                SamplingRate = _signal.SamplingRate,
                FrameDuration = frameDuration,
                HopDuration = hopDuration,
                ModulationFftSize = modulationFftSize,
                ModulationHopSize = modulationHopSize,
                FilterBank = _filterbank,
                Window = WindowTypes.Hamming
            };

            _extractor = new AmsExtractor(options);
            _features = _extractor.ComputeFrom(_signal);

            _featIndex = 0;

            infoLabel.Text = $"{_features.Count}x{_features[0].Length}";

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
                DrawModulationSpectraHerz(
                    _extractor.VectorsAtHerz(
                        _features, float.Parse(herzTextBox.Text)));
            }
            else
            {
                DrawModulationSpectrum(
                    _extractor.MakeSpectrum2D(_features[_featIndex]));
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

        private void DrawEnvelopes(float[][] envelopes)
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

        private void DrawModulationSpectrum(float[][] spectrum)
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

        private void DrawModulationSpectraHerz(List<float[]> spectra)
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
