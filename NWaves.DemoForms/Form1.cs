using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Audio.Interfaces;
using NWaves.Audio.Mci;
using NWaves.Signals;
using NWaves.Signals.Builders;
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class Form1 : Form
    {
        private DiscreteSignal _signal1;
        private DiscreteSignal _signal2;
        private DiscreteSignal _signal3;

        private readonly MciAudioPlayer _player = new MciAudioPlayer();
        private string _waveFileName;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            textBox1.Text = ofd.FileName;
            _waveFileName = ofd.FileName;

            using (var stream = new FileStream(ofd.FileName, FileMode.Open))
            {
                IAudioContainer waveFile = new WaveFile(stream);
                _signal1 = waveFile[Channels.Left];
            }

            DrawSignal(panel1, _signal1, 53);
        }

        private void DrawSignal(Control panel, DiscreteSignal signal, int step = 1)
        {
            var g = panel.CreateGraphics();

            g.Clear(Color.White);

            var offset = panel.Height / 2;

            Pen pen;

            if (panel == panel1)
            {
                pen = new Pen(Color.Blue);
            }
            else if (panel == panel2)
            {
                pen = new Pen(Color.Red);
            }
            else
            {
                pen = new Pen(Color.DarkGreen);
            }

            var i = 0;
            var x = 0;

            while (i < signal.Samples.Length)
            {
                if (Math.Abs(signal[i]*200) < panel.Height)
                {
                    g.DrawLine(pen, x, offset, x, (float) -signal[i]*200 + offset);
                    g.DrawEllipse(pen, x - 1, (int) (-signal[i]*200) + offset - 1, 3, 3);
                }
                x++;
                i += step;
                
            }

            pen.Dispose();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var sampleCount = int.Parse(textBox2.Text);

            switch (comboBox1.Text)
            {
                case "Sinusoid":
                    _signal2 = new SinusoidBuilder()
                                        .SetParameter("amp", 0.2)
                                        .SetParameter("freq", 200)
                                        .OfLength(sampleCount)
                                        .SampledAt(_signal1.SamplingRate)
                                        .Build();

                    listBox1.Items.Clear();
                    listBox1.Items.Add("Sinusoidal Builder:");
                    listBox1.Items.AddRange(new SinusoidBuilder().GetParametersInfo());

                    break;

                case "Sawtooth":
                    _signal2 = new SawtoothBuilder()
                                        .SetParameter("low", -0.2)
                                        .SetParameter("high", 0.5)
                                        .SetParameter("freq", 0.02)
                                        .OfLength(sampleCount)
                                        .SampledAt(_signal1.SamplingRate)
                                        .Build();

                    listBox1.Items.Clear();
                    listBox1.Items.Add("Sawtooth Builder:");
                    listBox1.Items.AddRange(new SawtoothBuilder().GetParametersInfo());

                    break;
            }

            _signal3 = _signal1.Superimpose(_signal2);

            DrawSignal(panel2, _signal2, 53);
            DrawSignal(panel3, _signal3, 53);

            var spectrum = Transform.MagnitudeSpectrum(_signal2[0, 512].Samples);
            var signal4 = new DiscreteSignal(spectrum.Select(s => s / 100), _signal2.SamplingRate);

            DrawSignal(panel4, signal4);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var param = int.Parse(textBox3.Text);

            switch (comboBox2.Text)
            {
                case "Delay by":
                    _signal2 = _signal2 + param;
                    break;

                case "Repeat times":
                    _signal2 = _signal2 * param;
                    break;
            }

            _signal3 = _signal1.Superimpose(_signal2);

            DrawSignal(panel2, _signal2, 53);
            DrawSignal(panel3, _signal3, 53);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var from = int.Parse(textBox4.Text);
            var to = int.Parse(textBox5.Text);

            _signal2 = _signal1[from, to];

            DrawSignal(panel2, _signal2, 53);
            DrawSignal(panel3, _signal3, 53);
        }

        private async void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_waveFileName);
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _player.Pause();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _player.Stop();
        }

        private void recordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var stream = new FileStream(@"d:\test.wav", FileMode.Create))
            {
                var waveFile = new WaveFile(_signal2);
                waveFile.SaveTo(stream);
            }
        }
    }
}
