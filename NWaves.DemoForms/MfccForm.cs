using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Multi;
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
                                                  samplingRate, 13, 512.0 / samplingRate, 0.01,
                                                  filterbank: filterbank,
                                                  //filterbankSize: 26,
                                                  //highFreq: 8000,
                                                  //preEmphasis: 0.97,
                                                  //lifterSize: 22,
                                                  //includeEnergy: true,
                                                  spectrumType: spectrumType,
                                                  nonLinearity: nonLinearity,
                                                  dctType: comboBoxDct.Text,
                                                  window: WindowTypes.Hamming,
                                                  logFloor: logFloor);

            _mfccVectors = mfccExtractor.ComputeFrom(_signal);

            //FeaturePostProcessing.NormalizeMean(_mfccVectors);        // optional
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


    // If you want to test MFCC against HTK =======================================================
    // keep in mind that HTK does the following pre-processing: zero-mean and pre-emphasis ========
    // (turn these settings off in HTK config if possible): =======================================

    // HTK does this pre-processing per frame instead of entire signal
    // (which is weird given that frames overlap).

    // Also: DON't normalize signal! If it's normalized then multiply by 32768 before processing:

    //      _signal *= 32768;
    //      _extractor.ComputeFrom(_signal);

    class MfccExtractorTestHtk : MfccExtractorHtk
    {
        public MfccExtractorTestHtk(int samplingRate,
                                    int featureCount,
                                    double frameDuration = 0.0256/*sec*/,
                                    double hopDuration = 0.010/*sec*/,
                                    int filterbankSize = 24,
                                    double lowFreq = 0,
                                    double highFreq = 0,
                                    int fftSize = 0,
                                    int lifterSize = 0,
                                    double preEmphasis = 0,
                                    bool includeEnergy = false,
                                    SpectrumType spectrumType = SpectrumType.Power,
                                    WindowTypes window = WindowTypes.Hamming)
            
            : base(samplingRate, featureCount, frameDuration, hopDuration, filterbankSize, lowFreq, highFreq, fftSize, lifterSize, preEmphasis, includeEnergy, spectrumType, window)
        {
        }

        /// <summary>
        /// HTK-style pre-processing (zero-mean and pre-emphasis)
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public override float[] ProcessFrame(float[] block)
        {
            // 1) HTK zero-mean:

            var frameSize = FrameSize;

            var mean = block.Take(frameSize).Average();

            for (var k = 0; k < frameSize; k++)
            {
                block[k] -= mean;
                block[k] += Math.Sign(block[k]) * 0.5f;
            }

            // 2) HTK pre-emphasis (it's different from conventional pre-emphasis!):

            // set base _preEmphasis field to 0 and do pre-emphasis here:

            var pre = 0.97f;

            for (var k = frameSize - 1; k >= 1; k--)
            {
                block[k] -= block[k - 1] * pre;
            }
            block[0] *= 1 - pre;


            // ...and now continue standard computations:

            return base.ProcessFrame(block);
        }

        /// <summary>
        /// True if computations can be done in parallel
        /// </summary>
        /// <returns></returns>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() =>
            new MfccExtractorTestHtk( SamplingRate,
                                      FeatureCount,
                                      FrameDuration,
                                      HopDuration,
                                      FilterBank.Length,
                                     _lowFreq,
                                     _highFreq,
                                     _blockSize,
                                     _lifterSize,
                                     _preEmphasis,
                                     _includeEnergy,
                                     _spectrumType,
                                     _window);
    }
}


// =================================================== TEST ParallelComputeFrom: ========================================================

//_mfccVectors = mfccExtractor.ComputeFrom(_signal);
//var mfccVectorsP = mfccExtractor.ParallelComputeFrom(_signal);

//for (var i = 0; i < _mfccVectors.Count; i++)
//{
//    for (var j = 0; j < _mfccVectors[i].Features.Length; j++)
//    {
//        if (Math.Abs(_mfccVectors[i].Features[j] - mfccVectorsP[i].Features[j]) > 1e-32f)
//        {
//            MessageBox.Show($"Nope: {i} - {j}");
//            return;
//        }

//        if (Math.Abs(_mfccVectors[i].TimePosition - mfccVectorsP[i].TimePosition) > 1e-32f)
//        {
//            MessageBox.Show($"Time: {i} - {j}");
//            return;
//        }
//    }
//}




// ====================================================== test PNCC: =============================================================

//var mfccExtractor = new PnccExtractor(_signal.SamplingRate,
//                                      13,
//                                      preEmphasis: 0.97,
//                                      fftSize: 1024,
//                                      window: WindowTypes.Hamming);

//_mfccVectors = mfccExtractor.ComputeFrom(_signal);

//            FeaturePostProcessing.NormalizeMean(_mfccVectors);
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




//            var sr = _signal.SamplingRate;

//            var melbands = FilterBanks.MelBands(26, 512, sr, 0, 8000);

//            // HTK, Kaldi:
//            var melbank = FilterBanks.Triangular(512, sr, melbands, null, Utils.Scale.HerzToMel);

//            // LIBROSA:
//            // var melbank = FilterBanks.Triangular(512, sr, melbands);