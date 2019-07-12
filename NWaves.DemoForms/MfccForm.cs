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

            comboBoxNonLinearity.Items.AddRange(Enum.GetNames(typeof(NonLinearityType)));
            comboBoxSpectrum.Items.AddRange(Enum.GetNames(typeof(SpectrumType)));

            comboBoxNonLinearity.SelectedIndex = 0;
            comboBoxSpectrum.SelectedIndex = 1;
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
                var waveFile = new WaveFile(stream, true);
                _signal = waveFile[Channels.Left];
            }

            buttonCompute_Click(this, null);    // :-D
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

        private void buttonCompute_Click(object sender, EventArgs e)
        {
            var filterCount = int.Parse(textBoxSize.Text);
            var samplingRate = _signal.SamplingRate;
            var fftSize = int.Parse(textBoxFftSize.Text);
            var lowFreq = float.Parse(textBoxLowFreq.Text);
            var highFreq = float.Parse(textBoxHighFreq.Text);

            Tuple<double, double, double>[] bands;
            float[][] filterbank = null;
            VtlnWarper vtln = null;

            if (checkBoxVtln.Checked)
            {
                var alpha = float.Parse(textBoxVtlnAlpha.Text);
                var vtlnLow = float.Parse(textBoxVtlnLow.Text);
                var vtlnHigh = float.Parse(textBoxVtlnHigh.Text);

                vtln = new VtlnWarper(alpha, lowFreq, highFreq, vtlnLow, vtlnHigh);
            }

            switch (comboBoxFilterbank.Text)
            {
                case "Mel":
                    bands = FilterBanks.MelBands(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    break;
                case "Mel Slaney":
                    bands = FilterBanks.MelBandsSlaney(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    filterbank = FilterBanks.MelBankSlaney(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxNormalize.Checked, vtln);
                    break;
                case "Bark":
                    bands = FilterBanks.BarkBands(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    break;
                case "Bark Slaney":
                    bands = FilterBanks.BarkBandsSlaney(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    filterbank = FilterBanks.BarkBankSlaney(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                case "Critical bands":
                    bands = FilterBanks.CriticalBands(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                case "Octave bands":
                    bands = FilterBanks.OctaveBands(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    break;
                case "ERB":
                    bands = null;
                    filterbank = FilterBanks.Erb(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                default:
                    bands = FilterBanks.HerzBands(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    break;
            }

            if (bands != null && filterbank == null)
            {
                switch (comboBoxShape.Text)
                {
                    case "Triangular":
                        filterbank = FilterBanks.Triangular(fftSize, samplingRate, bands, vtln, Utils.Scale.HerzToMel);
                        break;
                    case "Trapezoidal":
                        filterbank = FilterBanks.Trapezoidal(fftSize, samplingRate, bands, vtln);
                        break;
                    case "BiQuad":
                        filterbank = FilterBanks.BiQuad(fftSize, samplingRate, bands);
                        break;
                    default:
                        filterbank = FilterBanks.Rectangular(fftSize, samplingRate, bands, vtln);
                        break;
                }

                if (checkBoxNormalize.Checked) FilterBanks.Normalize(filterCount, bands, filterbank);
            }


            var spectrumType = (SpectrumType)comboBoxSpectrum.SelectedIndex;
            var nonLinearity = (NonLinearityType)comboBoxNonLinearity.SelectedIndex;
            var logFloor = float.Parse(textBoxLogFloor.Text);

            var mfccExtractor = new MfccExtractor(//samplingRate, 13, 0.025, 0.01,
                                                  samplingRate, 13, 512.0/samplingRate, 0.01,
                                                  filterbank: filterbank,
                                                  //preEmphasis: 0.97,
                                                  //includeEnergy: true,
                                                  spectrumType: spectrumType,
                                                  nonLinearity: nonLinearity,
                                                  dctType: comboBoxDct.Text,
                                                  window: WindowTypes.Hamming,
                                                  logFloor: logFloor);

            _mfccVectors = mfccExtractor.ComputeFrom(_signal);

            //FeaturePostProcessing.NormalizeMean(_mfccVectors);        // optional (but REQUIRED for PNCC!)
            //FeaturePostProcessing.AddDeltas(_mfccVectors);

            var header = mfccExtractor.FeatureDescriptions;
                                           //.Concat(mfccExtractor.DeltaFeatureDescriptions)
                                           //.Concat(mfccExtractor.DeltaDeltaFeatureDescriptions);

            FillFeaturesList(_mfccVectors, header);
            mfccListView.Items[0].Selected = true;

            melFilterBankPanel.Groups = mfccExtractor.FilterBank;

            mfccPanel.Line = _mfccVectors[0].Features;
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




//            //var vtln = new VtlnWarper(1.2, 0, 8000, 0, 8000);                     // alpha = 1.2
//            //var vtln = new VtlnWarper(0.85, 0, 8000, 0, (int)(8000 * 0.85));      // alpha = 0.85

//            var sr = _signal.SamplingRate;
//            var melbands = FilterBanks.MelBands(26, 512, sr, 0, 8000);
//            // HTK, Kaldi:
//            var melbank = FilterBanks.Triangular(512, sr, melbands, null, Utils.Scale.HerzToMel);
//            // LIBROSA:
//            // var melbank = FilterBanks.Triangular(512, sr, melbands);

//            // test normalization:
//            // FilterBanks.Normalize(26, melbands, melbank);


//            // we can easily change mel filters to bark filters, for example:

//            //var barkbands = FilterBanks.BarkBands(16, 512, sr, 100/*Hz*/, 6500/*Hz*/, overlap: false);
//            //var barkbank = FilterBanks.Triangular(512, sr, barkbands);


//            var mfccExtractor = new MfccExtractor(_signal.SamplingRate, 13, 0.025, 0.01,
//                                                  //filterbankSize: 26,
//                                                  //lowFreq: 100,
//                                                  //highFreq: 4200,
//                                                  filterbank: melbank,
//                                                  //filterbank: FilterBanks.MelBankSlaney(40, 512, _signal.SamplingRate),//, vtln: vtln),
//                                                  //filterbank: FilterBanks.BarkBankSlaney(15, 512, _signal.SamplingRate),
//                                                  lifterSize: 22,
//                                                  //preEmphasis: 0.97,
//                                                  //fftSize: 1024,
//                                                  //includeEnergy: true,
//                                                  spectrumType: SpectrumType.Power,
//                                                  nonLinearity: NonLinearityType.LogE,
//                                                  dctType: "2N",
//                                                  window: WindowTypes.Hamming,
//                                                  logFloor: 1.0f);

//// If you need to test MFCC against HTK =======================================================
//// keep in mind that HTK does the following pre-processing ====================================
//// (turn these settings off in HTK config if possible): =======================================

////_signal *= 32768;

////// 1) zero-mean:

////var mean = _signal.Samples.Average();

////for (var i = 0; i < _signal.Length; i++)
////{
////    _signal[i] -= mean;
////    _signal[i] += Math.Sign(_signal[i]) * 0.5f;
////}

////// 2) pre-emphasis (it's different from conventional pre-emphasis!):

////var pre = 0.97f;

////for (var i = _signal.Length - 1; i >= 1; i--)
////{
////    _signal[i] -= _signal[i - 1] * pre;
////}
////_signal[0] *= 1 - pre;


///* Actually, if we write the code above before MFCC extraction
// * the results will still be slightly different 
// * because HTK does this pre-processing per frame instead of entire signal
// * (which is weird given that frames overlap).
// * 
// * So if you really must stick to HTK scheme then copy MfccExtractor code
// * to another class and put this code instead of lines 351-362:
// * 
//        var mean = _block.Take(frameSize).Average();

//        for (var k = 0; k < frameSize; k++)
//        {
//            _block[k] -= mean;
//            _block[k] += Math.Sign(_block[k]) * 0.5f;
//        }

//        // 2) pre-emphasis (it's different from conventional pre-emphasis!):

//        var pre = 0.97f;

//        for (var k = frameSize - 1; k >= 1; k--)
//        {
//            _block[k] -= _block[k - 1] * pre;
//        }
//        _block[0] *= 1 - pre;
// *
// */

//// =============================================================================================


//// test PNCC:

////var mfccExtractor = new PnccExtractor(_signal.SamplingRate, 13,
////                          //filterbankSize: 40,
////                          //lowFreq: 100,
////                          //highFreq: 4200,
////                          //lifterSize: 22,
////                          //filterbank: barkbank,
////                          preEmphasis: 0.97,
////                          fftSize: 1024,
////                          //lifterSize: 0,
////                          window: WindowTypes.Hamming);




//_mfccVectors = mfccExtractor.ComputeFrom(_signal);

//            FeaturePostProcessing.NormalizeMean(_mfccVectors);        // optional (but REQUIRED for PNCC!)
//            FeaturePostProcessing.AddDeltas(_mfccVectors);


//            // ============== I use this code to test PNCC results (just ignore it))): ========================

//            //pnccVectors = new List<FeatureVector>();
//            //var vector = new FeatureVector() { Features = new float[13] };
//            //var pos = 1;

//            //using (var fs = new FileStream(@"E:\Projects\github\NWaves_Materials\pncc\esh_ru_0001.pncc", FileMode.Open))
//            //using (var br = new BinaryReader(fs))
//            //{
//            //    while (pos < 700)
//            //    {
//            //        br.ReadSingle();
//            //        for (var i = 0; i < 12; i++)
//            //        {
//            //            vector.Features[i] = br.ReadSingle();
//            //        }
//            //        pnccVectors.Add(vector);
//            //        vector = new FeatureVector() { Features = new float[13] };
//            //        pos++;
//            //    }
//            //}

//            //mfccPanel.Markline = pnccVectors[0].Features;

//            // ================================================================================================


//            var header = mfccExtractor.FeatureDescriptions
//                                           .Concat(mfccExtractor.DeltaFeatureDescriptions)
//                                           .Concat(mfccExtractor.DeltaDeltaFeatureDescriptions);

//            FillFeaturesList(_mfccVectors, header);
//            mfccListView.Items[0].Selected = true;

//            melFilterBankPanel.Groups = mfccExtractor.FilterBank;

//            mfccPanel.Line = _mfccVectors[0].Features;

//            //using (var csvFile = new FileStream("mfccs.csv", FileMode.Create))
//            //{
//            //    var serializer = new CsvFeatureSerializer(_mfccVectors, header);
//            //    await serializer.SerializeAsync(csvFile);
//            //}

