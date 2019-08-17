using NWaves.Filters.Adaptive;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Signals.Builders;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace NWaves.DemoForms
{
    public partial class AdaptiveFiltersForm : Form
    {
        DiscreteSignal _s;
        DiscreteSignal _noise;

        public AdaptiveFiltersForm()
        {
            InitializeComponent();
        }

        private void adaptButton_Click(object sender, EventArgs e)
        {
            var mu = float.Parse(muTextBox.Text);

            AdaptiveFilter filter;

            if (nlmsRadioButton.Checked)
                filter = new NlmsFilter(5, mu);
            else if (lmfRadioButton.Checked)
                filter = new LmfFilter(5, mu);
            else if (rlsRadioButton.Checked)
                filter = new RlsFilter(5, mu);
            else
                filter = new LmsFilter(5, mu);

            var a = Enumerable.Range(0, _noise.Length)
                              .Select(i => filter.Process(_noise[i], _s[i]))
                              .ToArray();
            
            linePlot1.Markline = a;

            weightsListBox.DataSource = new BindingList<float>(filter.Kernel);
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            _noise = sinRadioButton.Checked ? 
                        new SineBuilder()
                            .SetParameter("freq", 1000)
                            .OfLength(1000)
                            .SampledAt(16000)
                            .Build()
                        :
                        new WhiteNoiseBuilder()
                            .SetParameter("min", -0.5)
                            .SetParameter("max", 0.5)
                            .OfLength(1000)
                            .SampledAt(16000)
                            .Build();

            var fir = new FirFilter(new[] { 0.2, 1, -0.5, 0.5, 0.9 });

            _s = fir.ApplyTo(_noise);

            linePlot1.Stride = 5;
            linePlot1.Line = _s.Samples;
        }
    }
}
