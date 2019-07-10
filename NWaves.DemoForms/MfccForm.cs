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
using NWaves.Transforms;
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

            //var sr = 16000;
            //var barkbands = FilterBanks.MelBands(21, 1024, sr, 0, 8000);
            //var barkbank1 = FilterBanks.Triangular(1024, sr, barkbands);
            ////var barkbank1 = FilterBanks.Triangular(1024, sr, barkbands, mapper: Utils.Scale.HerzToMel);
            ////var barkbank1 = FilterBanks.MelBankSlaney(40, 1024, sr, 0, 8000);

            //var s = "";
            //for (var i = 0; i < barkbank1.Length; i++)
            //{
            //    var m = string.Join(", ", barkbank1[i].Select(b => b.ToString("0.0000000000", System.Globalization.CultureInfo.InvariantCulture)));
            //    s += m + ", ";
            //}

            //var f = File.CreateText("e:\\aaa.txt");

            //f.WriteLine(s.Remove(s.Length - 2));

            //f.Close();
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
            //var barkbands = FilterBanks.BarkBands(16, 512, sr, 100/*Hz*/, 6500/*Hz*/, overlap: false);
            //var barkbank = FilterBanks.Triangular(512, sr, barkbands);

            var vtln = new VtlnWarper(1.2, 0, 8000, 0, 8000);
            //var vtln = new VtlnWarper(0.85, 0, 8000, 0, (int)(8000 * 0.85));

            var sr = _signal.SamplingRate;
            var melbands = FilterBanks.MelBands(26, 512, sr, 0, 8000);
            var melbank = FilterBanks.Triangular(512, sr, melbands, null, Utils.Scale.HerzToMel);

            var mfccExtractor = new MfccExtractor(_signal.SamplingRate, 13, 0.025, 0.01,
                                                  //filterbankSize: 26,
                                                  //lowFreq: 100,
                                                  //highFreq: 4200,
                                                  filterbank: melbank,
                                                  //filterbank: FilterBanks.MelBankSlaney(40, 512, _signal.SamplingRate),//, vtln: vtln),
                                                  //filterbank: FilterBanks.BarkBankSlaney(15, 512, _signal.SamplingRate),
                                                  lifterSize: 22,
                                                  //preEmphasis: 0.97,
                                                  //fftSize: 1024,
                                                  //includeEnergy: true,
                                                  spectrumType: SpectrumType.Power,
                                                  nonLinearity: NonLinearityType.LogE,
                                                  dctType: "2N",
                                                  window: WindowTypes.Hamming,
                                                  logFloor: 1.0f);


            //var mfccExtractor = new PnccExtractor(_signal.SamplingRate, 13,
            //                          //filterbankSize: 40,
            //                          //lowFreq: 100,
            //                          //highFreq: 4200,
            //                          //lifterSize: 22,
            //                          //filterbank: barkbank,
            //                          preEmphasis: 0.97,
            //                          fftSize: 1024,
            //                          //lifterSize: 0,
            //                          window: WindowTypes.Hamming);

            // If you need to test MFCC against HTK =======================================================
            // keep in mind that HTK does the following pre-processing ====================================
            // (turn these settings off if possible): =====================================================

            _signal *= 32768;

            // 1) zero-mean:

            var mean = _signal.Samples.Average();

            for (var i = 0; i < _signal.Length; i++)
            {
                _signal[i] -= mean;
                _signal[i] += Math.Sign(_signal[i]) * 0.5f;
            }

            // 2) pre-emphasis (it's different from conventional pre-emphasis!):

            var pre = 0.97f;

            for (var i = _signal.Length - 1; i >= 1; i--)
            {
                _signal[i] -= _signal[i - 1] * pre;
            }
            _signal[0] *= 1.0f - pre;            // =============================================================================================

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

            //mfccPanel.Line = _mfccVectors[e.ItemIndex].Features;
            //mfccPanel.Markline = pnccVectors[e.ItemIndex].Features;

            // ================================================================================================
        }
    }
}
