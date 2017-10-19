using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.FeatureExtractors;
using NWaves.FeatureExtractors.Base;
using NWaves.Signals;

namespace NWaves.DemoForms
{
    public partial class MfccForm : Form
    {
        private DiscreteSignal _signal;

        public MfccForm()
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

            var mfccExtractor = new MfccExtractor(13, _signal.SamplingRate);
            
            var featureVectors = mfccExtractor.ComputeFrom(_signal);

            PlotMelFilterbanks(mfccExtractor.MelFilterBanks);
            FillFeaturesList(featureVectors);
        }

        private void PlotMelFilterbanks(double[][] filterbanks)
        {
            var g = panel1.CreateGraphics();
            g.Clear(Color.White);

            var rand = new Random();

            var offset = panel1.Height - 20;

            for (var j = 0; j < filterbanks.Length; j++)
            {
                var pen = new Pen(Color.FromArgb(rand.Next() % 255, rand.Next() % 255, rand.Next() % 255));

                var i = 1;
                var x = 2;

                while (i < filterbanks[j].Length)
                {
                    g.DrawLine(pen, 
                        x-2, (float)-filterbanks[j][i-1] * 100 + offset, 
                        x,   (float)-filterbanks[j][i] * 100 + offset);
                    x += 2;
                    i++;
                }

                pen.Dispose();
            }
        }

        private void FillFeaturesList(IEnumerable<FeatureVector> featureVectors)
        {
            listView1.Clear();

            listView1.Columns.Add("time", 50);

            foreach (var feat in featureVectors.First().Features)
            {
                listView1.Columns.Add("coeff", 70);
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
