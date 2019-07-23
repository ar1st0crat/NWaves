using NWaves.Transforms.Wavelets;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace NWaves.DemoForms
{
    public partial class WaveletForm : Form
    {
        public WaveletForm()
        {
            InitializeComponent();
        }

        private void WaveletForm_Load(object sender, EventArgs e)
        {
            comboBoxFamily.Items.AddRange(Enum.GetNames(typeof(WaveletFamily)));
            comboBoxTaps.Items.AddRange(Enumerable.Range(1, 20).Select(i => i.ToString()).ToArray());
        }

        private void buttonCompute_Click(object sender, EventArgs e)
        {
            var size = int.Parse(textBoxSize.Text);
            var family = (WaveletFamily)comboBoxFamily.SelectedIndex;
            var taps = comboBoxTaps.SelectedIndex + 1;

            var wavelet = new Wavelet(family, taps);

            var fwt = new Fwt(size, wavelet);

            var output = new float[size];
            var reconstructed = new float[size];

            fwt.Direct(Enumerable.Range(0, size).Select(x => (float)x).ToArray(), output);
            fwt.Inverse(output, reconstructed);

            var res = string.Join("\r\n", output.Select(o => o.ToString()));
            textBoxResult.Text = res;

            var inv = string.Join("\r\n", reconstructed.Select(o => o.ToString()));
            textBoxResultInv.Text = inv;

            labelWaveletName.Text = wavelet.Name;

            linePlotWavelet.Thickness = 2;
            linePlotWavelet.Stride = 8;
            linePlotWavelet.Line = wavelet.LoD;
            linePlotWavelet.Markline = wavelet.HiD;

            var coeffs = string.Join("\r\n", wavelet.LoD.Select(o => o.ToString()));
            textBoxCoeffs.Text = coeffs;
        }
    }
}
