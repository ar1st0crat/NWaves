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
using NWaves.Windows;

namespace NWaves.DemoForms
{
    public partial class MfccForm : Form
    {
        private DiscreteSignal _signal;
        private List<FeatureVector> _mfccVectors;

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

            //var lines = new List<string>();

            //using (var fs = new FileStream(@"E:\Projects\Science\PNCC_C\esh_ru_0001.pncc", FileMode.Open))
            //using (var br = new BinaryReader(fs))
            //{
            //    var length = br.ReadInt32();
            //    MessageBox.Show(length + " size");

            //    var s = "";
            //    for (var i = 0; i < length; i++)
            //    {
            //        s += br.ReadSingle().ToString("F4") + " ";
            //    }
            //    lines.Add(s + "\r\n");
            //}

            //var txt = File.CreateText(@"E:\Projects\Science\PNCC_C\esh_ru_0001.pncc.txt");
            //foreach (var line in lines)
            //{
            //    txt.WriteLine(line);
            //    txt.WriteLine();
            //}
            //txt.Close();

            //var gammatoneFilterBank = new double[40][];

            //using (var fs = new FileStream(@"e:\GTFB.bin", FileMode.Open))
            //using (var br = new BinaryReader(fs))
            //{
            //    for (var i = 0; i < 40; i++)
            //    {
            //        gammatoneFilterBank[i] = new double[513];
            //        for (var j = 0; j < 512; j++)
            //        {
            //            gammatoneFilterBank[i][j] = br.ReadDouble();
            //        }
            //    }
            //}

            //var mfccExtractor = new MfccExtractor(13, _signal.SamplingRate,
            //                                      windowSize: 0.03,
            //                                      overlapSize: 0.015,
            //                                      melFilterbankSize: 20,
            //                                      //lowFreq: 100,
            //                                      //highFreq: 3200,
            //                                      lifterSize: 22,
            //                                      preEmphasis: 0.95,
            //                                      window: WindowTypes.Hamming);
            var mfccExtractor = new PnccExtractor(13, _signal.SamplingRate, preEmphasis: 0.97);
            //mfccExtractor.GammatoneFilterBank = gammatoneFilterBank;

            _mfccVectors = mfccExtractor.ComputeFrom(_signal).ToList();

            FillFeaturesList(_mfccVectors, mfccExtractor.FeatureDescriptions);
            mfccListView.Items[0].Selected = true;

            PlotMelFilterbank(mfccExtractor.GammatoneFilterBank);//PlotMelFilterbank(mfccExtractor.MelFilterBank);
            PlotMfcc(_mfccVectors[0].Features);
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
            PlotMfcc(_mfccVectors[e.ItemIndex].Features);
        }

        private void PlotMelFilterbank(double[][] filterbank)
        {
            var g = melFilterBankPanel.CreateGraphics();
            g.Clear(Color.White);

            var rand = new Random();

            var offset = melFilterBankPanel.Height - 20;

            for (var j = 0; j < filterbank.Length; j++)
            {
                var pen = new Pen(Color.FromArgb(rand.Next() % 255, rand.Next() % 255, rand.Next() % 255));

                var i = 1;
                var x = 2;

                while (i < filterbank[j].Length)
                {
                    g.DrawLine(pen, 
                        x-2, (float)-filterbank[j][i-1] * 100 + offset, 
                        x,   (float)-filterbank[j][i] * 100 + offset);
                    x += 2;
                    i++;
                }

                pen.Dispose();
            }
        }

        private void PlotMfcc(double[] mfcc, bool includeFirstCoeff = false)
        {
            var g = mfccPanel.CreateGraphics();
            g.Clear(Color.White);

            var xOffset = 30;
            var yOffset = mfccPanel.Height / 2;

            var stride = 20;

            var blackPen = new Pen(Color.Black);
            g.DrawLine(blackPen, xOffset, yOffset, xOffset + mfcc.Length * stride, yOffset);
            g.DrawLine(blackPen, xOffset, xOffset, xOffset, mfccPanel.Height - xOffset);
            blackPen.Dispose();

            var pen = new Pen(Color.Green, 3);

            var i = includeFirstCoeff ? 1 : 2;
            var x = xOffset + stride;

            for (; i < mfcc.Length; i++)
            {
                g.DrawLine(pen, x - stride, (float)-mfcc[i - 1] * 1 + yOffset, x, (float)-mfcc[i] * 1 + yOffset);
                x += stride;
            }

            pen.Dispose();
        }
    }
}
