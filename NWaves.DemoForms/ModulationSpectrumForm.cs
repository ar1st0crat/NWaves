using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.Filters.Fda;
using NWaves.Operations;
using NWaves.Signals;

namespace NWaves.DemoForms
{
    public partial class ModulationSpectrumForm : Form
    {
        private DiscreteSignal _signal;
        private MsExtractor _extractor;

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
            
            switch (filterbankComboBox.Text)
            {
                case "Mel":
                    _filterbank = FilterBanks.Mel(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                case "Bark":
                    _filterbank = FilterBanks.Bark(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                case "Critical bands (rectangular)":
                    _filterbank = FilterBanks.CriticalBandsRectangular(fftSize, samplingRate, lowFreq, highFreq);
                    break;
                case "Critical bands (BiQuad)":
                    var q = double.Parse(filterQTextBox.Text);
                    _filterbank = FilterBanks.CriticalBands(fftSize, samplingRate, lowFreq, highFreq, q);
                    break;
                case "ERB":
                    _filterbank = FilterBanks.Erb(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                default:
                    _filterbank = FilterBanks.Fourier(filterCount, fftSize);
                    break;
            }

            band1ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band2ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band3ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band4ComboBox.DataSource = Enumerable.Range(1, filterCount).ToArray();
            band1ComboBox.Text = "1";
            band2ComboBox.Text = "2";
            band3ComboBox.Text = "3";
            band4ComboBox.Text = "4";

            DrawFilterbank(_filterbank);
        }

        private void computeButton_Click(object sender, EventArgs e)
        {
            var windowSize = double.Parse(analysisFftTextBox.Text);
            var overlapSize = double.Parse(hopSizeTextBox.Text);
            var modulationFftSize = int.Parse(longTermFftSizeTextBox.Text);
            var modulationHopSize = int.Parse(longTermHopSizeTextBox.Text);

            var mfccExtractor = new PnccExtractor(13, _signal.SamplingRate, windowSize: windowSize, overlapSize: overlapSize);
            var vectors = mfccExtractor.ComputeFrom(_signal);
            
            //_extractor = new MsExtractor(_signal.SamplingRate,
            //                             windowSize, overlapSize, 
            //                             modulationFftSize, modulationHopSize,
            //                             filterbank: _filterbank);
            _extractor = new MsExtractor(_signal.SamplingRate,
                                         windowSize, overlapSize,
                                         modulationFftSize, modulationHopSize,
                                         featuregram: vectors.Select(v => v.Features));
            var features = _extractor.ComputeFrom(_signal);

            DrawEnvelopes(_extractor.Envelopes);
            DrawModulationSpectrum(_extractor.MakeSpectrum2D(features));
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

        private void DrawFilterbank(double[][] filterbank)
        {
            var g = filterbankPanel.CreateGraphics();
            g.Clear(Color.White);

            var rand = new Random();

            var offset = filterbankPanel.Height - 20;

            for (var j = 0; j < filterbank.Length; j++)
            {
                var pen = new Pen(Color.FromArgb(rand.Next() % 255, rand.Next() % 255, rand.Next() % 255));

                var i = 1;
                var x = 2;

                while (i < filterbank[j].Length)
                {
                    g.DrawLine(pen,
                        x - 2, (float)-filterbank[j][i - 1] * 100 + offset,
                        x, (float)-filterbank[j][i] * 100 + offset);
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

            var stride = 1;
            for (var i = 0; i < 4; i++)
            {
                var en = envNo[i] - 1;

                g.DrawLine(blackPen, xOffset, offsets[i], envelopesPanel.Width - xOffset, offsets[i]);
                g.DrawLine(blackPen, xOffset, offsets[i] - 70, xOffset, offsets[i]);

                var x = 1;
                for (var j = stride; j < envelopes[en].Length; j += stride)
                {
                    g.DrawLine(pen,
                        xOffset + x - 1, (float)-envelopes[en][j-stride] * 1.5f + offsets[i],
                        xOffset + x,     (float)-envelopes[en][j] * 1.5f + offsets[i]);
                    x++;
                }
            }

            pen.Dispose();
        }

        private void DrawModulationSpectrum(double[][] spectrum)
        {
            var g = modulationSpectrumPanel.CreateGraphics();
            g.Clear(Color.White);

            var spectrumBitmap = new Bitmap(spectrum[0].Length, spectrum.Length);

            for (var i = 0; i < spectrum.Length; i++)
            {
                for (var j = 0; j < spectrum[i].Length; j++)
                {
                    spectrumBitmap.SetPixel(j, spectrum.Length - 1 - i, Color.FromArgb((byte)(spectrum[i][j] * 5), 0, 0));
                    
                }
            }

            g.DrawImage(spectrumBitmap, 25, 25, modulationSpectrumPanel.Width - 25, modulationSpectrumPanel.Height - 25);
            //spectrumBitmap.Width * 2, spectrumBitmap.Height * 16);
        }

        #endregion
    }
}
