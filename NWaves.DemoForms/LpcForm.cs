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
        private DiscreteSignal _signal;

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
            
            var lpcExtractor = new LpcExtractor(16, hopSize: 512);

            var featureVectors = lpcExtractor.ComputeFrom(_signal).ToList();

            FillFeaturesList(featureVectors, lpcExtractor.FeatureDescriptions);


            // draw estimated spectrum

            var pos = 70;

            var spectrum = Transform.LogPowerSpectrum(_signal[512 * pos, 512 * (pos + 1)].Samples);

            var vector = featureVectors[pos].Features;
            var gain = Math.Sqrt(vector[0]);
            vector[0] = 1.0;

            var lpcFilter = new IirFilter(new [] { gain }, vector, 512);

            var lpcSpectrum = lpcFilter.FrequencyResponse.Magnitude.Samples;

            for (var i = 0; i < lpcSpectrum.Length; i++)
            {
                lpcSpectrum[i] = 20 * Math.Log10(lpcSpectrum[i] * lpcSpectrum[i]);
            }

            PlotLpcSpectrum(spectrum, lpcSpectrum);
        }

        private void PlotLpcSpectrum(double[] spectrum, double[] lpcSpectrum)
        {
            var g = panel1.CreateGraphics();
            g.Clear(Color.White);

            var pen = new Pen(Color.Blue);

            var offset = panel1.Height / 2 - 20;

            var x = 2;
            for (var j = 1; j < spectrum.Length; j++)
            {
                g.DrawLine(pen,
                           x - 2, (float)-spectrum[j - 1] + offset,
                           x,     (float)-spectrum[j] + offset);
                x += 2;
            }

            pen.Dispose();
            

            pen = new Pen(Color.Red, 2);

            x = 2;
            for (var j = 1; j < lpcSpectrum.Length / 2; j++)
            {
                g.DrawLine(pen,
                           x - 2, (float)-lpcSpectrum[j - 1] + offset,
                           x,     (float)-lpcSpectrum[j] + offset);
                x += 2;
            }

            pen.Dispose();
        }

        private void FillFeaturesList(IEnumerable<FeatureVector> featureVectors, 
                                      IEnumerable<string> featureDescriptions)
        {
            listView1.Clear();

            listView1.Columns.Add("time", 50);

            foreach (var name in featureDescriptions)
            {
                listView1.Columns.Add(name, 70);
            }

            foreach (var vector in featureVectors)
            {
                var item = new ListViewItem { Text = vector.TimePosition.ToString() };
                item.SubItems.AddRange(vector.Features.Select(f => f.ToString("F4")).ToArray());

                listView1.Items.Add(item);
            }
        }
    }
}
