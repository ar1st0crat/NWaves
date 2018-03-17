using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.Audio.Interfaces;
using NWaves.Audio.Mci;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
using NWaves.Filters.Fda;
using NWaves.Operations;
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class FiltersForm : Form
    {
        private LtiFilter _filter;

        private DiscreteSignal _signal;
        private DiscreteSignal _filteredSignal;

        private readonly Stft _stft = new Stft(256, fftSize: 256);

        private string _waveFileName;

        private readonly MciAudioPlayer _player = new MciAudioPlayer();


        public FiltersForm()
        {
            InitializeComponent();

            magnitudeResponsePanel.Stride = 2;
            magnitudeResponsePanel.Thickness = 2;
            magnitudeResponsePanel.ForeColor = Color.SeaGreen;
            phaseResponsePanel.Stride = 2;
            phaseResponsePanel.Thickness = 2;
            phaseResponsePanel.ForeColor = Color.SeaGreen;

            signalBeforeFilteringPanel.Gain = 80;
            signalAfterFilteringPanel.Gain = 80;
        }

        private void buttonAnalyzeFilter_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            switch (filterTypesComboBox.Text)
            {
                case "Custom IIR":
                    AnalyzeCustomIirFilter();
                    break;
                case "Custom FIR":
                    AnalyzeCustomFirFilter();
                    break;
                case "BiQuad LP":
                case "BiQuad HP":
                case "BiQuad BP":
                case "BiQuad notch":
                case "BiQuad allpass":
                case "BiQuad peaking":
                case "BiQuad lowshelf":
                case "BiQuad highshelf":
                    AnalyzeBiQuadFilter(filterTypesComboBox.Text);
                    break;
                case "Moving average":
                    AnalyzeMovingAverageFilter();
                    break;
                case "Moving average recursive":
                    AnalyzeRecursiveMovingAverageFilter();
                    break;
                case "Pre-emphasis":
                    AnalyzePreemphasisFilter();
                    break;
                case "Butterworth":
                    _filter = new ButterworthFilter(0.1, 5);

                    numeratorListBox.DataSource = (_filter as IirFilter).B;
                    denominatorListBox.DataSource = (_filter as IirFilter).A;

                    filterParamsDataGrid.RowCount = 2;
                    filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
                    filterParamsDataGrid.Rows[0].Cells[1].Value = "5";
                    filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
                    filterParamsDataGrid.Rows[1].Cells[1].Value = "0,1";
                    orderNumeratorTextBox.Text = "6";
                    orderDenominatorTextBox.Text = "6";

                    break;
                case "Custom LP/HP":
                    _filter = DesignFilter.FirLp(17, 0.1);
                    
                    //using (var csv = new FileStream("fir.csv", FileMode.Open))
                    //{
                    //    _filter = FirFilter.FromCsv(csv);
                    //}

                    numeratorListBox.DataSource = (_filter as FirFilter)?.Kernel;

                    filterParamsDataGrid.RowCount = 2;
                    filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
                    filterParamsDataGrid.Rows[0].Cells[1].Value = "17";
                    filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
                    filterParamsDataGrid.Rows[1].Cells[1].Value = "0,1";
                    orderNumeratorTextBox.Text = "0";
                    orderDenominatorTextBox.Text = "17";
                    break;
            }

            
            magnitudeResponsePanel.Line = _filter.FrequencyResponse().Magnitude;
            phaseResponsePanel.Line = _filter.FrequencyResponse().Phase;

            poleZeroPanel.Zeros = _filter.Zeros;
            poleZeroPanel.Poles = _filter.Poles;

            Cursor.Current = Cursors.Default;
        }

        private void filterTypesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (filterTypesComboBox.Text)
            {
                case "Custom IIR":
                    orderNumeratorTextBox.Enabled = true;
                    orderDenominatorTextBox.Enabled = true;
                    changeOrderButton.Enabled = true;
                    break;
                case "Custom FIR":
                    orderNumeratorTextBox.Enabled = true;
                    orderDenominatorTextBox.Enabled = false;
                    changeOrderButton.Enabled = true;
                    orderDenominatorTextBox.Text = "0";
                    break;
                default:
                    orderNumeratorTextBox.Enabled = false;
                    orderDenominatorTextBox.Enabled = false;
                    changeOrderButton.Enabled = false;
                    break;
            }

            filterParamsDataGrid.RowCount = 0;
        }

        private void changeOrderButton_Click(object sender, EventArgs e)
        {
            var b = int.Parse(orderNumeratorTextBox.Text) + 1;
            var a = int.Parse(orderDenominatorTextBox.Text) + 1;

            filterParamsDataGrid.RowCount = b + a;

            var pos = 0;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "b0";
            filterParamsDataGrid.Rows[0].Cells[1].Value = 1;
            pos++;
            for (var i = 1; i < b; i++, pos++)
            {
                filterParamsDataGrid.Rows[pos].Cells[0].Value = "b" + i;
                filterParamsDataGrid.Rows[pos].Cells[1].Value = 0;
            }
            filterParamsDataGrid.Rows[pos].Cells[0].Value = "a0";
            filterParamsDataGrid.Rows[pos].Cells[1].Value = 1;
            pos++;
            for (var i = 1; i < a; i++, pos++)
            {
                filterParamsDataGrid.Rows[pos].Cells[0].Value = "a" + i;
                filterParamsDataGrid.Rows[pos].Cells[1].Value = 0;
            }
        }

        #region filter analysis

        private void AnalyzeCustomIirFilter()
        {
            var b = new List<double>();
            var a = new List<double>();

            if (filterParamsDataGrid.RowCount == 0)
            {
                b.AddRange(new[] { 1, -0.4, 0.6 });
                a.AddRange(new[] { 1,  0.4, 0.2 });
            }
            else
            {
                for (var i = 0; i < filterParamsDataGrid.RowCount; i++)
                {
                    var param = filterParamsDataGrid.Rows[i].Cells[0].Value;
                    if (param.ToString().StartsWith("b"))
                    {
                        b.Add(Convert.ToDouble(filterParamsDataGrid.Rows[i].Cells[1].Value));
                    }
                    else
                    {
                        a.Add(Convert.ToDouble(filterParamsDataGrid.Rows[i].Cells[1].Value));
                    }
                }
            }

            _filter = new IirFilter(b, a);

            filterParamsDataGrid.RowCount = a.Count + b.Count;
            var pos = 0;
            for (var i = 0; i < b.Count; i++, pos++)
            {
                filterParamsDataGrid.Rows[pos].Cells[0].Value = "b" + i;
                filterParamsDataGrid.Rows[pos].Cells[1].Value = b[i];
            }
            for (var i = 0; i < a.Count; i++, pos++)
            {
                filterParamsDataGrid.Rows[pos].Cells[0].Value = "a" + i;
                filterParamsDataGrid.Rows[pos].Cells[1].Value = a[i];
            }
            numeratorListBox.DataSource = (_filter as IirFilter).B;
            denominatorListBox.DataSource = (_filter as IirFilter).A;
        }

        private void AnalyzeCustomFirFilter()
        {
            var b = new List<double>();

            var size = filterParamsDataGrid.RowCount;
            if (size == 0)
            {
                b.AddRange(new []{ 1, 0.4, -0.6 });
            }
            else
            {
                for (var i = 0; i < filterParamsDataGrid.RowCount; i++)
                {
                    var param = filterParamsDataGrid.Rows[i].Cells[0].Value;
                    if (param.ToString().StartsWith("b"))
                    {
                        b.Add(Convert.ToDouble(filterParamsDataGrid.Rows[i].Cells[1].Value));
                    }
                }
            }

            _filter = new FirFilter(b);

            filterParamsDataGrid.RowCount = b.Count + 1;
            for (var i = 0; i < b.Count; i++)
            {
                filterParamsDataGrid.Rows[i].Cells[0].Value = "b" + i;
                filterParamsDataGrid.Rows[i].Cells[1].Value = b[i];
            }
            filterParamsDataGrid.Rows[b.Count].Cells[0].Value = "a0";
            filterParamsDataGrid.Rows[b.Count].Cells[1].Value = 1.0;

            numeratorListBox.DataSource = (_filter as FirFilter).Kernel;
            denominatorListBox.DataSource = new[] { 1.0 };
        }

        private void AnalyzeBiQuadFilter(string filterType)
        {
            var freq = 0.1;
            var q = 1.0;
            var gain = 9.0;

            for (var i = 0; i < filterParamsDataGrid.RowCount; i++)
            {
                if (filterParamsDataGrid.Rows[i].Cells[0].Value.ToString() == "freq")
                {
                    freq = Convert.ToDouble(filterParamsDataGrid.Rows[i].Cells[1].Value);
                }
                if (filterParamsDataGrid.Rows[i].Cells[0].Value.ToString() == "q")
                {
                    q = Convert.ToDouble(filterParamsDataGrid.Rows[i].Cells[1].Value);
                }
                if (filterParamsDataGrid.Rows[i].Cells[0].Value.ToString() == "gain")
                {
                    gain = Convert.ToDouble(filterParamsDataGrid.Rows[i].Cells[1].Value);
                }
            }
            
            string[] parameters = { "freq", "q" };
            double[] values = { freq, q };

            switch (filterType)
            {
                case "BiQuad LP":
                    _filter = new LowPassFilter(freq, q);
                    break;
                case "BiQuad HP":
                    _filter = new HighPassFilter(freq, q);
                    break;
                case "BiQuad BP":
                    _filter = new BandPassFilter(freq, q);
                    break;
                case "BiQuad notch":
                    _filter = new NotchFilter(freq, q);
                    break;
                case "BiQuad allpass":
                    _filter = new AllPassFilter(freq, q);
                    break;
                case "BiQuad peaking":
                    _filter = new PeakFilter(freq, q, gain);
                    parameters = new[] { "freq", "q", "gain" };
                    values = new[] { freq, q, gain };
                    break;
                case "BiQuad lowshelf":
                    _filter = new LowShelfFilter(freq, q, gain);
                    parameters = new[] { "freq", "q", "gain" };
                    values = new[] { freq, q, gain };
                    break;
                case "BiQuad highshelf":
                    _filter = new HighShelfFilter(freq, q, gain);
                    parameters = new[] { "freq", "q", "gain" };
                    values = new[] { freq, q, gain };
                    break;
            }

            numeratorListBox.DataSource = (_filter as IirFilter)?.B;
            denominatorListBox.DataSource = (_filter as IirFilter)?.A;
            filterParamsDataGrid.RowCount = parameters.Length;
            for (var i = 0; i < parameters.Length; i++)
            {
                filterParamsDataGrid.Rows[i].Cells[0].Value = parameters[i];
                filterParamsDataGrid.Rows[i].Cells[1].Value = values[i];
            }
            orderNumeratorTextBox.Text = "2";
            orderDenominatorTextBox.Text = "2";
        }

        private void AnalyzeMovingAverageFilter()
        {
            var size = 3;
            if (filterParamsDataGrid.RowCount > 0)
            {
                size = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (size - 1).ToString();
            orderDenominatorTextBox.Text = "0";

            _filter = new MovingAverageFilter(size);

            numeratorListBox.DataSource = (_filter as FirFilter).Kernel;
            denominatorListBox.DataSource = new[] { 1.0 };

            filterParamsDataGrid.RowCount = 1;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "size";
            filterParamsDataGrid.Rows[0].Cells[1].Value = size;
        }

        private void AnalyzeRecursiveMovingAverageFilter()
        {
            var size = 3;
            if (filterParamsDataGrid.RowCount > 0)
            {
                size = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (size - 1).ToString();
            orderDenominatorTextBox.Text = "0";

            _filter = new MovingAverageRecursiveFilter(size);

            numeratorListBox.DataSource = (_filter as IirFilter).B;
            denominatorListBox.DataSource = (_filter as IirFilter).A;

            filterParamsDataGrid.RowCount = 1;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "size";
            filterParamsDataGrid.Rows[0].Cells[1].Value = size;
        }

        private void AnalyzePreemphasisFilter()
        {
            var pre = 0.95;
            if (filterParamsDataGrid.RowCount > 0)
            {
                pre = Convert.ToDouble(filterParamsDataGrid.Rows[0].Cells[1].Value);
            }

            _filter = new PreEmphasisFilter(pre);

            numeratorListBox.DataSource = (_filter as FirFilter).Kernel;
            denominatorListBox.DataSource = new[] { 1.0 };

            filterParamsDataGrid.RowCount = 1;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "a";
            filterParamsDataGrid.Rows[0].Cells[1].Value = pre.ToString("F2");
            orderNumeratorTextBox.Text = "1";
            orderDenominatorTextBox.Text = "0";
        }

        #endregion

        #region filtering

        private void overlapAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringOptions.OverlapAdd);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void overlapSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringOptions.OverlapSave);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void differenceEquationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringOptions.DifferenceEquation);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        #endregion

        #region resampling

        private void interpolateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            var factor = int.Parse(resampleTextBox.Text);

            _filteredSignal = Operation.Interpolate(_signal, factor);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void decimateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            var factor = int.Parse(resampleTextBox.Text);

            _filteredSignal = Operation.Decimate(_signal, factor);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void customToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            Cursor.Current = Cursors.WaitCursor;

            var rate = int.Parse(resampleTextBox.Text);

            _filteredSignal = Operation.Resample(_signal, rate);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);

            Cursor.Current = Cursors.Default;
        }

        #endregion

        #region File menu

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _waveFileName = ofd.FileName;

            using (var stream = new FileStream(_waveFileName, FileMode.Open))
            {
                IAudioContainer waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Left];
            }

            signalBeforeFilteringPanel.Signal = _signal;
            spectrogramBeforeFilteringPanel.Spectrogram = _stft.Spectrogram(_signal);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var stream = new FileStream(sfd.FileName, FileMode.Create))
            {
                var waveFile = new WaveFile(_filteredSignal);
                waveFile.SaveTo(stream);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion

        #region playback

        private async void playSignalButton_Click(object sender, EventArgs e)
        {
            await _player.PlayAsync(_waveFileName);
        }

        private async void playFilteredSignalButton_Click(object sender, EventArgs e)
        {
            // create temporary file
            const string tmpFilename = "tmpfiltered.wav";
            using (var stream = new FileStream(tmpFilename, FileMode.Create))
            {
                var waveFile = new WaveFile(_filteredSignal);
                waveFile.SaveTo(stream);
            }

            await _player.PlayAsync(tmpFilename);
        }

        #endregion
    }
}
