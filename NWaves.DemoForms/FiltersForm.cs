using System;
using System.Windows.Forms;
using NWaves.Filters.Base;

namespace NWaves.DemoForms
{
    public partial class FiltersForm : Form
    {
        private IFilter _filter;

        public FiltersForm()
        {
            InitializeComponent();
        }

        private void buttonAnalyzeFilter_Click(object sender, EventArgs e)
        {
            _filter = new FirFilter(new[] {1.0, -0.95});
            //_filter.FrequencyResponse;
        }
    }
}
