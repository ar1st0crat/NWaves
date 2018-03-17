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

            mfccPanel.ForeColor = Color.SeaGreen;
            mfccPanel.Thickness = 2;
            mfccPanel.Stride = 20;
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
                var waveFile = new WaveFile(stream, true);
                _signal = waveFile[Channels.Left];
            }

            var mfccExtractor = new MfccExtractor(13,
                                                      //windowSize: 0.03125,
                                                      //overlapSize: 0.015625,
                                                      melFilterbankSize: 20,
                                                      //lowFreq: 100,
                                                      //highFreq: 4200,
                                                      //lifterSize: 22,
                                                      preEmphasis: 0.95f,
                                                      window: WindowTypes.Hamming);
            _mfccVectors = mfccExtractor.ComputeFrom(_signal);
            //FeaturePostProcessing.NormalizeMean(_mfccVectors);
            //FeaturePostProcessing.AddDeltas(_mfccVectors);

            FillFeaturesList(_mfccVectors, mfccExtractor.FeatureDescriptions);
            mfccListView.Items[0].Selected = true;

            melFilterBankPanel.Groups = mfccExtractor.FilterBank;
            
            mfccPanel.Line = _mfccVectors[0].Features;

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
            mfccPanel.Line = _mfccVectors[e.ItemIndex].Features;
        }
    }
}
