using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Multi;
using NWaves.Signals;

namespace NWaves.DemoForms
{
    public partial class FeaturesForm : Form
    {
        private DiscreteSignal _signal;
        private FeatureVector[] _vectors;

        public FeaturesForm()
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

            using (var stream = new FileStream(ofd.FileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Left];
            }

            var windowSize = (float) 4096 / _signal.SamplingRate;
            var hopSize = (float) 2048 / _signal.SamplingRate;

            var tdExtractor = new TimeDomainFeaturesExtractor("all", windowSize, hopSize);
            var spectralExtractor = new SpectralFeaturesExtractor("all", windowSize, hopSize);

            var tdVectors = tdExtractor.ComputeFrom(_signal);
            var spectralVectors = spectralExtractor.ComputeFrom(_signal);

            _vectors = FeaturePostProcessing.Join(tdVectors, spectralVectors);

            //FeaturePostProcessing.NormalizeMean(_vectors);
            //FeaturePostProcessing.AddDeltas(_vectors);

            var descriptions = tdExtractor.FeatureDescriptions
                                          .Concat(spectralExtractor.FeatureDescriptions);

            FillFeaturesList(_vectors, descriptions);
        }

        private void FillFeaturesList(IEnumerable<FeatureVector> featureVectors,
                                      IEnumerable<string> featureDescriptions)
        {
            featuresListView.Clear();
            featuresListView.Columns.Add("time", 50);

            foreach (var feat in featureDescriptions)
            {
                featuresListView.Columns.Add(feat, 70);
            }

            foreach (var vector in featureVectors)
            {
                var item = new ListViewItem { Text = vector.TimePosition.ToString("F4") };
                item.SubItems.AddRange(vector.Features.Select(f => f.ToString("F4")).ToArray());

                featuresListView.Items.Add(item);
            }
        }

        private void featuresListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column == 0)
            {
                return;
            }

            featurePlotPanel.Stride = 1;
            featurePlotPanel.Line = _vectors.Select(v => v.Features[e.Column - 1]).ToArray();
        }
    }
}
