using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NWaves.Audio;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class ModulationSpectrumForm : Form
    {
        private DiscreteSignal _signal;

        public ModulationSpectrumForm()
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

            var spectra = new double[_signal.Samples.Length / 1024 - 1][];

            var i = 0;
            var pos = 0;
            while (i + 2048 < _signal.Samples.Length)
            {
                var fragment = _signal[i, i + 2048];
                var spectrum = Transform.PowerSpectrum(fragment.Samples, 2048);

                spectra[pos++] = spectrum;

                i += 1024;
            }

            DrawSpectra(spectra);
        }

        private void DrawSpectra(double[][] spectra)
        {
            var g = envelopesPanel.CreateGraphics();
            g.Clear(Color.White);

            var rand = new Random();

            var offsets = Enumerable.Range(0, 5).Select(i => 100 + i * 80).ToArray();

            var pen = new Pen(Color.Blue);

            for (var j = 1; j < spectra.Length - 1; j++)
            {
                for (var i = 0; i < 5; i++)
                {
                    g.DrawLine(pen,
                        j - 1, (float)-spectra[j - 1][i + 70] * 20 + offsets[i],
                        j,     (float)-spectra[j][i + 70] * 20 + offsets[i]);
                }
            }

            pen.Dispose();
        }
    }
}
