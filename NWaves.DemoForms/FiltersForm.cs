using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.DemoForms
{
    public partial class FiltersForm : Form
    {
        private LtiFilter _filter;

        public FiltersForm()
        {
            InitializeComponent();
        }

        private void buttonAnalyzeFilter_Click(object sender, EventArgs e)
        {
            switch (comboBox1.Text)
            {
                case "Custom IIR":
                    //_filter = new IirFilter(new[] { 1, 0.95 }, new[] { 1.0 });
                    _filter = new IirFilter(new[] { 1, 0.4 }, new[] { 1, -0.6, 0.2 });
                    break;
                case "Custom FIR":
                    _filter = new FirFilter(new[] {1, 0.95});
                    break;
                case "BiQuad":
                    _filter = new BiQuadFilter(freq: 0.2, width: 0.3);
                    break;
                case "Moving average":
                    _filter = new MovingAverageFilter(11);
                    break;
            }

            DrawFrequencyResponse();

            var r = new Random();
            var kernel = new double[231];
            for (var i = 0; i < kernel.Length; i++)
                kernel[i] = r.NextDouble() - 0.5;

            var filter = new IirFilter(kernel,
                                       new[] { 1, -0.6, 0.2, -0.3, 0.5, -0.1, 0.7, -0.6, 0.3, -0.4, 0.5, 0.2, 0.3, 0.6, -0.9, 0.4 });

            var signal = new DiscreteSignal(22050, 22050 * 30);

            var summary = "";

            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var sw3 = new Stopwatch();

            

            sw2.Start();
            for (var i = 0; i < 2; i++)
                filter.ApplyFilterLinearBuffer(signal);
            sw2.Stop();

            summary += "Linear buffer: " + sw2.ElapsedTicks + "\n";
            
            sw3.Start();
            for (var i = 0; i < 2; i++)
                filter.ApplyFilterDirectly(signal);
            sw3.Stop();

            summary += "Directly: " + sw3.ElapsedTicks + "\n";

            sw1.Start();
            for (var i = 0; i < 2; i++)
                filter.ApplyFilterCircularBuffer(signal);
            sw1.Stop();

            summary += "Circular buffer: " + sw1.ElapsedTicks + "\n";

            MessageBox.Show(summary);
        }

        private void DrawFrequencyResponse(int step = 2)
        {
            var g = panel1.CreateGraphics();

            g.Clear(Color.White);

            var offset = panel1.Height - 20;

            var pen = new Pen(Color.Blue);

            var magnitudeResponse = _filter.FrequencyResponse.Magnitude;

            var i = 0;
            var x = 0;

            while (i < magnitudeResponse.Samples.Length / 2)
            {
                if (Math.Abs(magnitudeResponse[i] * 80) < panel1.Height)
                {
                    g.DrawLine(pen, x, offset, x, (float)-magnitudeResponse[i] * 80 + offset);
                    g.DrawEllipse(pen, x - 1, (int)(-magnitudeResponse[i] * 80) + offset - 1, 3, 3);
                }
                x += step;
                i++;

            }


            g = panel2.CreateGraphics();

            g.Clear(Color.White);

            var phaseResponse = _filter.FrequencyResponse.Phase;

            offset = 20;

            i = 0;
            x = 0;

            while (i < phaseResponse.Samples.Length / 2)
            {
                if (Math.Abs(phaseResponse[i] * 80) < panel1.Height)
                {
                    g.DrawLine(pen, x, offset, x, (float)-phaseResponse[i] * 80 + offset);
                    g.DrawEllipse(pen, x - 1, (int)(-phaseResponse[i] * 80) + offset - 1, 3, 3);
                }
                x += step;
                i++;
            }

            pen.Dispose();
        }
    }
}
