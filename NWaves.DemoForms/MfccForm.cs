using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Serializers;
using NWaves.Signals;
using NWaves.Windows;

namespace NWaves.DemoForms
{
    public partial class MfccForm : Form
    {
        private DiscreteSignal _signal;
        private List<FeatureVector> _mfccVectors;

        public MfccForm()
        {
            InitializeComponent();
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
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

            var mfccExtractor = new MfccExtractor(13, _signal.SamplingRate,
                                                      //windowSize: 0.05,
                                                      //overlapSize: 0.025,
                                                      melFilterbankSize: 20,
                                                      //lowFreq: 100,
                                                      //highFreq: 4200,
                                                      lifterSize: 22,
                                                      preEmphasis: 0.95,
                                                      window: WindowTypes.Hamming);
            _mfccVectors = mfccExtractor.ComputeFrom(_signal);
            //FeaturePostProcessing.NormalizeMean(_mfccVectors);
            FeaturePostProcessing.AddDeltas(_mfccVectors);

            FillFeaturesList(_mfccVectors, mfccExtractor.FeatureDescriptions);
            mfccListView.Items[0].Selected = true;

            PlotMelFilterbank(mfccExtractor.MelFilterBank);
            PlotMfcc(_mfccVectors[0].Features);

            using (var csvFile = new FileStream("mfccs.csv", FileMode.Create))
            {
                var header =  mfccExtractor.FeatureDescriptions
                                           .Concat(mfccExtractor.DeltaFeatureDescriptions)
                                           .Concat(mfccExtractor.DeltaDeltaFeatureDescriptions);

                var serializer = new CsvFeatureSerializer(_mfccVectors, header);
                await serializer.SerializeAsync(csvFile);
            }
        }

        private void FillFeaturesList(IEnumerable<FeatureVector> featureVectors,
                                      IEnumerable<string> featureDescriptions)
        {
            mfccListView.Clear();

            mfccListView.Columns.Add("time", 50);

            foreach (var feat in featureDescriptions)
            {
                mfccListView.Columns.Add(feat, 70);
            }

            foreach (var vector in featureVectors)
            {
                var item = new ListViewItem { Text = vector.TimePosition.ToString() };
                item.SubItems.AddRange(vector.Features.Select(f => f.ToString("F4")).ToArray());

                mfccListView.Items.Add(item);
            }
        }

        private void mfccListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            PlotMfcc(_mfccVectors[e.ItemIndex].Features);
        }

        private void PlotMelFilterbank(double[][] filterbank)
        {
            var g = melFilterBankPanel.CreateGraphics();
            g.Clear(Color.White);

            var rand = new Random();

            var offset = melFilterBankPanel.Height - 20;

            for (var j = 0; j < filterbank.Length; j++)
            {
                var pen = new Pen(Color.FromArgb(rand.Next() % 255, rand.Next() % 255, rand.Next() % 255));

                var i = 1;
                var x = 2;

                while (i < filterbank[j].Length)
                {
                    g.DrawLine(pen, 
                        x-2, (float)-filterbank[j][i - 1] * 100 + offset, 
                        x,   (float)-filterbank[j][i] * 100 + offset);
                    x += 2;
                    i++;
                }

                pen.Dispose();
            }
        }

        private void PlotMfcc(double[] mfcc, bool includeFirstCoeff = false)
        {
            var g = mfccPanel.CreateGraphics();
            g.Clear(Color.White);

            var xOffset = 30;
            var yOffset = mfccPanel.Height / 2;

            var stride = 20;

            var blackPen = new Pen(Color.Black);
            g.DrawLine(blackPen, xOffset, yOffset, xOffset + mfcc.Length * stride, yOffset);
            g.DrawLine(blackPen, xOffset, xOffset, xOffset, mfccPanel.Height - xOffset);
            blackPen.Dispose();

            var pen = new Pen(Color.Green, 3);

            var i = includeFirstCoeff ? 1 : 2;
            var x = xOffset + stride;

            for (; i < mfcc.Length; i++)
            {
                g.DrawLine(pen, x - stride, (float)-mfcc[i - 1] + yOffset, x, (float)-mfcc[i] + yOffset);
                x += stride;
            }

            pen.Dispose();
        }
    }
}
