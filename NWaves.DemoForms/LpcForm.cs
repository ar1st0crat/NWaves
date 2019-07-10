using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.DemoForms
{
    public partial class LpcForm : Form
    {
        private const double FrameDuration = 0.032;
        private const double HopDuration = 0.010;

        private DiscreteSignal _signal;
        private List<FeatureVector> _lpcVectors;

        private RealFft _fft;


        public LpcForm()
        {
            InitializeComponent();
            lpcPanel.ForeColor = Color.SeaGreen;
            lpcPanel.Stride = 20;
            lpcPanel.Thickness = 2;
            spectrumPanel.Stride = 2;
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

            _fft = new RealFft(512);

            //var lpcExtractor = new LpcExtractor(_signal.SamplingRate, 16, FrameDuration, HopDuration);
            //var lpcExtractor = new LpccExtractor(_signal.SamplingRate, 16, FrameDuration, HopDuration, lifterSize: 0);

            var sr = _signal.SamplingRate;
            var melbands = FilterBanks.MelBands(24, 512, sr, 0, 8000);
            var melbank = FilterBanks.Triangular(512, sr, melbands, null, Utils.Scale.HerzToMel);

            var lpcExtractor = new PlpExtractor(_signal.SamplingRate, 12,
                                      //filterbankSize: 23,
                                      lpcOrder: 12,
                                      //lowFreq: 100,
                                      //highFreq: 4200,
                                      //lifterSize: 22,
                                      filterbank: melbank,
                                      centerFrequencies: melbands.Select(m => m.Item2).ToArray(),
                                      window: WindowTypes.Rectangular);
                                      //preEmphasis: 0.95,
                                      //rasta: 0.94,
                                      //fftSize: 1024);

            //var data = new float[] { 1, 7, 2, 5, 4, 9, 1, 2, 3, 4, 5, 3, 4, 7, 6, 5, 1, 2, 3, 4, 5, 7, 7, 2, 3, 1, 9 }.PadZeros(512);
            //_signal = new DiscreteSignal(16000, data);

            // HTK result:
            // -0.328354, -0.0626681, -0.0529092, -0.035697, -0.0780158, -0.0472787, -0.114684, -0.0103448, -0.072201, -0.0479003, -0.0231306, 0.00597705

            _lpcVectors = lpcExtractor.ComputeFrom(_signal);

            FillFeaturesList(_lpcVectors, lpcExtractor.FeatureDescriptions);
            lpcListView.Items[0].Selected = true;

            spectrumPanel.Line = ComputeSpectrum(0);
            spectrumPanel.Markline = EstimateSpectrum(0);
            spectrumPanel.ToDecibel();

            lpcPanel.Line = _lpcVectors[0].Features.Skip(1).ToArray();
        }

        float[] ComputeSpectrum(int idx)
        {
            var pos = (int)(_signal.SamplingRate * HopDuration * idx);

            return _fft.PowerSpectrum(_signal[pos, pos + 512], normalize: false)
                       .Samples;
        }

        float[] EstimateSpectrum(int idx)
        {
            var lpcc = _lpcVectors[idx].Features;
            var lpc = new float[lpcc.Length];
            var gain = MathUtils.CepstrumToLpc(lpcc, lpc);

            var vector = lpc.ToDoubles();
            vector[0] = 1.0;

            var lpcTf = new TransferFunction(new double[] { Math.Sqrt(gain) }, vector);

            return lpcTf.FrequencyResponse().Power.ToFloats();

            //var vector = _lpcVectors[idx].Features.ToDoubles();  // make new copy of array of features
            //var gain = Math.Sqrt(vector[0]);
            //vector[0] = 1.0;

            //var lpcTf = new TransferFunction(new[] { gain }, vector);

            //return lpcTf.FrequencyResponse().Power.ToFloats();
        }

        private void FillFeaturesList(IEnumerable<FeatureVector> featureVectors, 
                                      IEnumerable<string> featureDescriptions)
        {
            lpcListView.Clear();
            lpcListView.Columns.Add("time", 50);

            foreach (var name in featureDescriptions)
            {
                lpcListView.Columns.Add(name, 70);
            }

            foreach (var vector in featureVectors)
            {
                var item = new ListViewItem { Text = vector.TimePosition.ToString() };
                item.SubItems.AddRange(vector.Features.Select(f => f.ToString("F4")).ToArray());

                lpcListView.Items.Add(item);
            }
        }

        private void lpcListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var pos = e.ItemIndex;

            spectrumPanel.Line = ComputeSpectrum(pos);
            spectrumPanel.Markline = EstimateSpectrum(pos);
            spectrumPanel.ToDecibel();

            lpcPanel.Line = _lpcVectors[pos].Features.Skip(1).ToArray();
        }
    }
}
