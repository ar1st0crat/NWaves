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
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Windows;

namespace NWaves.DemoForms
{
    public partial class MfccForm : Form
    {
        private DiscreteSignal _signal;
        private List<FeatureVector> _mfccVectors, pnccVectors;

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

            // we can easily change mel filters to bark filters, for example:

            //var sr = _signal.SamplingRate;
            //var barkbands = FilterBanks.Bark1Bands(16, 512, sr, 100/*Hz*/, 6500/*Hz*/, overlap: false);
            //var barkbank = FilterBanks.Triangular(512, sr, barkbands);

            var mfccExtractor = new MfccExtractor(_signal.SamplingRate, 13,
                                                  //filterbankSize: 40,
                                                  //lowFreq: 100,
                                                  //highFreq: 4200,
                                                  //lifterSize: 22,
                                                  //filterbank: barkbank,
                                                  preEmphasis: 0.95,
                                                  //fftSize: 1024,
                                                  window: WindowTypes.Hamming);

            //var mfccExtractor = new PlpExtractor(_signal.SamplingRate, 12,
            //                                      filterbankSize: 23,
            //                                      lpcOrder: 8,
            //                                      //lowFreq: 100,
            //                                      //highFreq: 4200,
            //                                      //lifterSize: 22,
            //                                      //filterbank: barkbank,
            //                                      //preEmphasis: 0.95,
            //                                      //rasta: 0.94,
            //                                      //fftSize: 1024,
            //                                      window: WindowTypes.Hamming);

            _mfccVectors = mfccExtractor.ComputeFrom(_signal);

            //FeaturePostProcessing.NormalizeMean(_mfccVectors);        // optional (but REQUIRED for PNCC!)
            //FeaturePostProcessing.AddDeltas(_mfccVectors);


            // ============== I use this code to test PNCC results (just ignore it))): ========================

            //pnccVectors = new List<FeatureVector>();
            //var vector = new FeatureVector() { Features = new float[13] };
            //var pos = 1;

            //using (var fs = new FileStream(@"E:\Projects\github\NWaves_Materials\pncc\esh_ru_0001.pncc", FileMode.Open))
            //using (var br = new BinaryReader(fs))
            //{
            //    while (pos < 700)
            //    {
            //        br.ReadSingle();
            //        for (var i = 0; i < 12; i++)
            //        {
            //            vector.Features[i] = br.ReadSingle();
            //        }
            //        pnccVectors.Add(vector);
            //        vector = new FeatureVector() { Features = new float[13] };
            //        pos++;
            //    }
            //}

            //mfccPanel.Markline = pnccVectors[0].Features;

            // ================================================================================================


            FillFeaturesList(_mfccVectors, mfccExtractor.FeatureDescriptions);
            mfccListView.Items[0].Selected = true;

            melFilterBankPanel.Groups = mfccExtractor.FilterBank;
            
            mfccPanel.Line = _mfccVectors[0].Features;

            using (var csvFile = new FileStream("mfccs.csv", FileMode.Create))
            {
                var header = mfccExtractor.FeatureDescriptions;
                                           //.Concat(mfccExtractor.DeltaFeatureDescriptions)
                                           //.Concat(mfccExtractor.DeltaDeltaFeatureDescriptions);

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

            // ============== I use this code to test PNCC results (just ignore it))): ========================

            // mfccPanel.Line = _mfccVectors[e.ItemIndex + 2].Features;
            // mfccPanel.Markline = pnccVectors[e.ItemIndex].Features;

            // ================================================================================================
        }
    }
}
