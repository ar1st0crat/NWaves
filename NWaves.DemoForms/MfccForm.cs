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
using NWaves.FeatureExtractors.Options;
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
        private List<float[]> _mfccVectors, pnccVectors;

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
                var waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Left];
            }

            buttonCompute_Click(this, null);    // :-D
        }

        private void FillFeaturesList(IList<float[]> featureVectors,
                                      IList<string> featureDescriptions,
                                      IList<double> timeMarkers)
        {
            mfccListView.Clear();
            mfccListView.Columns.Add("time", 50);

            foreach (var feat in featureDescriptions)
            {
                mfccListView.Columns.Add(feat, 70);
            }

            for (var i = 0; i < featureVectors.Count; i++)
            {
                var item = new ListViewItem { Text = timeMarkers[i].ToString("F4") };
                item.SubItems.AddRange(featureVectors[i].Select(f => f.ToString("F4")).ToArray());

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

            (double, double, double)[] bands;
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
                    bands = FilterBanks.MelBands(filterCount, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    break;
                case "Mel Slaney":
                    bands = FilterBanks.MelBandsSlaney(filterCount, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    filterbank = FilterBanks.MelBankSlaney(filterCount, fftSize, samplingRate, lowFreq, highFreq, checkBoxNormalize.Checked, vtln);
                    break;
                case "Bark":
                    bands = FilterBanks.BarkBands(filterCount, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    break;
                case "Bark Slaney":
                    bands = FilterBanks.BarkBandsSlaney(filterCount, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    filterbank = FilterBanks.BarkBankSlaney(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                case "Critical bands":
                    bands = FilterBanks.CriticalBands(filterCount, samplingRate, lowFreq, highFreq);
                    break;
                case "Octave bands":
                    bands = FilterBanks.OctaveBands(filterCount, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
                    break;
                case "ERB":
                    bands = null;
                    filterbank = FilterBanks.Erb(filterCount, fftSize, samplingRate, lowFreq, highFreq);
                    break;
                default:
                    bands = FilterBanks.HerzBands(filterCount, samplingRate, lowFreq, highFreq, checkBoxOverlap.Checked);
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

            var mfccOptions = new MfccOptions
            {
                SamplingRate = samplingRate,
                FeatureCount = 13,
                FrameDuration = 512.0 / samplingRate,
                HopDuration = 0.01,
                FilterBank = filterbank,
                SpectrumType = spectrumType,
                NonLinearity = nonLinearity,
                DctType = comboBoxDct.Text,
                Window = WindowTypes.Hamming,
                LogFloor = logFloor,
                //FilterBankSize = 26,
                //HighFrequency = 6000,
                //PreEmphasis = 0.97,
                //LifterSize = 22,
                //IncludeEnergy = true,
                //LogEnergyFloor = 1e-10
            };

            var mfccExtractor = new MfccExtractor(mfccOptions);
            _mfccVectors = mfccExtractor.ComputeFrom(_signal);

            //FeaturePostProcessing.NormalizeMean(_mfccVectors);        // optional
            //FeaturePostProcessing.AddDeltas(_mfccVectors);

            var header = mfccExtractor.FeatureDescriptions;
                                           //.Concat(mfccExtractor.DeltaFeatureDescriptions)
                                           //.Concat(mfccExtractor.DeltaDeltaFeatureDescriptions);

            FillFeaturesList(_mfccVectors, header, mfccExtractor.TimeMarkers(_mfccVectors.Count));
            mfccListView.Items[0].Selected = true;

            melFilterBankPanel.Groups = mfccExtractor.FilterBank;

            mfccPanel.Line = _mfccVectors[0];
        }

        private void mfccListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            mfccPanel.Line = _mfccVectors[e.ItemIndex];

            // ============== I use this code to test PNCC results (just ignore it))): ========================

            //mfccPanel.Line = _mfccVectors[e.ItemIndex];
            //mfccPanel.Markline = pnccVectors[e.ItemIndex];

            // ================================================================================================
        }
    }


    // If you want to test MFCC against HTK =======================================================
    // keep in mind that HTK does the following pre-processing: zero-mean and pre-emphasis ========
    // (turn these settings off in HTK config if possible): =======================================

    // HTK does this pre-processing per frame instead of entire signal
    // (which is weird given that frames overlap).

    // Also: HTK DOESN't normalize signal! If it's normalized then multiply by 32768 before processing:

    //      _signal *= 32768;
    //      _extractor.ComputeFrom(_signal);

    class MfccExtractorTestHtk : MfccExtractor
    {
        private readonly float[] _hammingWin;

        public MfccExtractorTestHtk(MfccOptions options) : base(options)
        {
            _hammingWin = Window.OfType(WindowTypes.Hamming, FrameSize);
        }

        /// <summary>
        /// HTK-style pre-processing (zero-mean and pre-emphasis)
        /// </summary>
        /// <param name="block"></param>
        /// <param name="features"></param>
        public override void ProcessFrame(float[] block, float[] features)
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


            // 3) apply hamming window:

            block.ApplyWindow(_hammingWin);

            
            // ...and now continue standard computations:

            base.ProcessFrame(block, features);
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
            new MfccExtractorTestHtk(
                new MfccOptions
                {
                    SamplingRate = SamplingRate,
                    FeatureCount = FeatureCount,
                    FrameDuration = FrameDuration,
                    HopDuration = HopDuration,
                    FilterBank = FilterBank,
                    FilterBankSize = FilterBank.Length,
                    FftSize = _blockSize,
                    LifterSize = _lifterSize,
                    PreEmphasis = _preEmphasis,
                    IncludeEnergy = _includeEnergy,
                    LogEnergyFloor = _logEnergyFloor,
                    SpectrumType = _spectrumType,
                    Window = _window
                });
    }
}


// =================================================== TEST ParallelComputeFrom: ========================================================

//_mfccVectors = mfccExtractor.ComputeFrom(_signal);
//var mfccVectorsP = mfccExtractor.ParallelComputeFrom(_signal);

//for (var i = 0; i < _mfccVectors.Count; i++)
//{
//    for (var j = 0; j < _mfccVectors[i].Length; j++)
//    {
//        if (Math.Abs(_mfccVectors[i][j] - mfccVectorsP[i][j]) > 1e-32f)
//        {
//            MessageBox.Show($"Nope: {i} - {j}");
//            return;
//        }
//    }
//}




// ====================================================== test PNCC: =============================================================

//var mfccExtractor = new PnccExtractor(
//    new PnccOptions
//    {
//        SamplingRate = _signal.SamplingRate,
//        FeatureCount = 13,
//        PreEmphasis = 0.97,
//        FftSize = 1024,
//        Window = WindowTypes.Hamming
//    });

//_mfccVectors = mfccExtractor.ComputeFrom(_signal);

//            FeaturePostProcessing.NormalizeMean(_mfccVectors);


//            // ============== I use this code to test PNCC results (just ignore it))): ========================

//            pnccVectors = new List<float[]>();
//            var vector = new float[13];
//var pos = 1;

//            using (var fs = new FileStream(@"E:\Projects\github\NWaves_Materials\pncc\esh_ru_0001.pncc", FileMode.Open))
//            using (var br = new BinaryReader(fs))
//            {
//                while (pos< 700)
//                {
//                    br.ReadSingle();
//                    for (var i = 0; i< 12; i++)
//                    {
//                        vector[i] = br.ReadSingle();
//                    }
//                    pnccVectors.Add(vector);
//                    vector = new float[13];
//                    pos++;
//                }
//            }

//            mfccPanel.Markline = pnccVectors[0];

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