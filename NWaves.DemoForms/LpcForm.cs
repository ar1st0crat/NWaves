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
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class LpcForm : Form
    {
        private const double WindowSize = 0.032;
        private const double OverlapSize = 0.010;

        private DiscreteSignal _signal;
        private List<FeatureVector> _lpcVectors;

        public LpcForm()
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
            
            var lpcExtractor = new LpcExtractor(16, _signal.SamplingRate, WindowSize, OverlapSize);

            _lpcVectors = lpcExtractor.ComputeFrom(_signal).ToList();

            FillFeaturesList(_lpcVectors, lpcExtractor.FeatureDescriptions);
            lpcListView.Items[0].Selected = true;

            var spectrum = ComputeSpectrum(0);
            var lpcSpectrum = EstimateSpectrum(0);
            PlotLpcSpectrum(spectrum, lpcSpectrum);
            PlotLpc(_lpcVectors[0].Features);
        }

        double[] ComputeSpectrum(int idx)
        {
            var pos = (int)(_signal.SamplingRate * OverlapSize * idx);
            return Transform.LogPowerSpectrum(_signal[pos, pos + 512].Samples);
        }

        double[] EstimateSpectrum(int idx)
        {
            var vector = _lpcVectors[idx].Features.ToArray();
            var gain = Math.Sqrt(vector[0]);
            vector[0] = 1.0;

            var lpcFilter = new IirFilter(new[] { gain }, vector);

            return lpcFilter.FrequencyResponse().LogPower;
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
            PlotLpcSpectrum(ComputeSpectrum(pos), EstimateSpectrum(pos));
            PlotLpc(_lpcVectors[pos].Features);
        }

        private void PlotLpcSpectrum(double[] spectrum, double[] lpcSpectrum)
        {
            var g = spectrumPanel.CreateGraphics();
            g.Clear(Color.White);

            var offset = spectrumPanel.Height / 2 - 20;

            var pen = new Pen(Color.Blue);
            var x = 2;
            for (var j = 1; j < spectrum.Length; j++)
            {
                g.DrawLine(pen, x - 2, (float)-spectrum[j - 1] + offset, x, (float)-spectrum[j] + offset);
                x += 2;
            }
            pen.Dispose();
            
            pen = new Pen(Color.Red, 2);
            x = 2;
            for (var j = 1; j < lpcSpectrum.Length / 2; j++)
            {
                g.DrawLine(pen, x - 2, (float)-lpcSpectrum[j - 1] + offset, x, (float)-lpcSpectrum[j] + offset);
                x += 2;
            }
            pen.Dispose();
        }

        private void PlotLpc(double[] lpc, bool includeFirstCoeff = false)
        {
            var g = lpcPanel.CreateGraphics();
            g.Clear(Color.White);

            var xOffset = 30;
            var yOffset = lpcPanel.Height / 2;

            var stride = 20;

            var blackPen = new Pen(Color.Black);
            g.DrawLine(blackPen, xOffset, yOffset, xOffset + lpc.Length * stride, yOffset);
            g.DrawLine(blackPen, xOffset, xOffset, xOffset, lpcPanel.Height - xOffset);
            blackPen.Dispose();

            var pen = new Pen(Color.Green, 3);

            var i = includeFirstCoeff ? 1 : 2;
            var x = xOffset + stride;

            for (; i < lpc.Length; i++)
            {
                g.DrawLine(pen, x - stride, 50 * (float)-lpc[i - 1] * 1 + yOffset, x, 50 * (float)-lpc[i] * 1 + yOffset);
                x += stride;
            }

            pen.Dispose();
        }
    }
}
