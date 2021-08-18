using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NWaves.Signals;
using NWaves.Audio;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
using NWaves.Filters.Fda;
using NWaves.Operations;
using NWaves.Transforms;
using NWaves.Utils;
using HighPassFilter = NWaves.Filters.BiQuad.HighPassFilter;
using LowPassFilter = NWaves.Filters.BiQuad.LowPassFilter;
using System.Linq;

namespace NWaves.DemoForms
{
    public partial class FiltersForm : Form
    {
        private LtiFilter _filter;

        private DiscreteSignal _signal;
        private DiscreteSignal _filteredSignal;

        private readonly Stft _stft = new Stft(256);

        private string _waveFileName;
        private short _bitDepth;

        private readonly MemoryStreamPlayer _player = new MemoryStreamPlayer();


        public FiltersForm()
        {
            InitializeComponent();

            magnitudeResponsePanel.Stride = 2;
            magnitudeResponsePanel.Thickness = 2;
            magnitudeResponsePanel.ForeColor = Color.SeaGreen;
            phaseResponsePanel.Stride = 1;
            phaseResponsePanel.Thickness = 2;
            phaseResponsePanel.ForeColor = Color.SeaGreen;

            signalBeforeFilteringPanel.Gain = 80;
            signalAfterFilteringPanel.Gain = 80;

            zpIterationsTextBox.Text = MathUtils.PolyRootsIterations.ToString();
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
                case "One-pole LP":
                    _filter = new Filters.OnePole.LowPassFilter(0.25);
                    break;
                case "One-pole HP":
                    _filter = new Filters.OnePole.HighPassFilter(0.25);
                    break;
                case "Comb feed-forward":
                    _filter = new CombFeedforwardFilter(500);
                    break;
                case "Comb feed-back":
                    _filter = new CombFeedbackFilter(1800);
                    break;
                case "Moving average":
                    AnalyzeMovingAverageFilter();
                    break;
                case "Moving average recursive":
                    AnalyzeRecursiveMovingAverageFilter();
                    break;
                case "Savitzky-Golay":
                    AnalyzeSavitzkyGolayFilter();
                    break;
                case "Pre-emphasis":
                    AnalyzePreemphasisFilter();
                    break;
                case "De-emphasis":
                    _filter = new DeEmphasisFilter();
                    break;
                case "DC removal":
                    _filter = new DcRemovalFilter();
                    break;
                case "RASTA":
                    _filter = new RastaFilter();
                    break;
                case "Butterworth":
                    AnalyzeButterworthFilter();
                    break;
                case "Elliptic":
                    AnalyzeEllipticFilter();
                    break;
                case "Chebyshev-I":
                    AnalyzeChebyshevIFilter();
                    break;
                case "Chebyshev-II":
                    AnalyzeChebyshevIIFilter();
                    break;
                case "Bessel":
                    AnalyzeBesselFilter();
                    break;
                case "Thiran":
                    AnalyzeThiranFilter();
                    break;
                case "Equiripple LP":
                    AnalyzeEquirippleLpFilter();
                    break;
                case "Equiripple BS":
                    AnalyzeEquirippleBsFilter();
                    break;
                case "Custom LP/HP":
                    AnalyzeCustomLpFilter();
                    break;
                case "Custom BP/BR":
                    AnalyzeCustomBandpassFilter();
                    break;
            }

            // we can load TF from csv file:

            //using (var csv = new FileStream("fir.csv", FileMode.Open))
            //{
            //    _filter = new FirFilter(TransferFunction.FromCsv(csv));
            //}

            var tf = _filter.Tf;

            // we can save TF to csv file:

            //using (var csv = new FileStream("fir.csv", FileMode.Create))
            //{
            //    tf.ToCsv(csv);
            //}

            magnitudeResponsePanel.Line = tf.FrequencyResponse().Magnitude.ToFloats();
            UpdatePhaseResponse();

            // adjust this if you need finer precision:
            tf.CalculateZpIterations = int.Parse(zpIterationsTextBox.Text);

            if (tf.Numerator.Length + tf.Denominator.Length < 70)
            {
                poleZeroPanel.Zeros = tf.Zeros;
                poleZeroPanel.Poles = tf.Poles;
            }

            numeratorListBox.DataSource = tf.Numerator;
            denominatorListBox.DataSource = tf.Denominator;

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

        private void phaseViewComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePhaseResponse();
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

        private void UpdatePhaseResponse()
        {
            var tf = _filter.Tf;

            var fr = tf.FrequencyResponse();

            switch (phaseViewComboBox.Text)
            {
                case "Phase unwrapped":
                    phaseResponsePanel.Line = fr.PhaseUnwrapped.ToFloats();
                    break;
                case "Group delay":
                    phaseResponsePanel.Line = tf.GroupDelay(256).ToFloats();
                    // or like this:
                    // fr.GroupDelay.ToFloats();
                    break;
                case "Phase delay":
                    phaseResponsePanel.Line = tf.PhaseDelay(256).ToFloats();
                    // or like this:
                    // fr.PhaseDelay.ToFloats();
                    break;
                default:
                    phaseResponsePanel.Line = fr.Phase.ToFloats();
                    break;
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
                        b.Add(Convert.ToSingle(filterParamsDataGrid.Rows[i].Cells[1].Value));
                    }
                    else
                    {
                        a.Add(Convert.ToSingle(filterParamsDataGrid.Rows[i].Cells[1].Value));
                    }
                }
            }

            // lose some precision:

            _filter = new IirFilter(b, a);

            // double precision:

            // _filter = new IirFilter(new TransferFunction(b.ToArray(), a.ToArray()));

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
                    _filter = new Filters.BiQuad.BandPassFilter(freq, q);
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

            filterParamsDataGrid.RowCount = 1;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "size";
            filterParamsDataGrid.Rows[0].Cells[1].Value = size;
        }

        private void AnalyzeSavitzkyGolayFilter()
        {
            var size = 9;
            if (filterParamsDataGrid.RowCount > 0)
            {
                size = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (size - 1).ToString();
            orderDenominatorTextBox.Text = "0";

            _filter = new SavitzkyGolayFilter(size);

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

            filterParamsDataGrid.RowCount = 1;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "a";
            filterParamsDataGrid.Rows[0].Cells[1].Value = pre.ToString("F2");
            orderNumeratorTextBox.Text = "1";
            orderDenominatorTextBox.Text = "0";
        }

        private void AnalyzeButterworthFilter()
        {
            var order = 5;
            var freq = 0.1;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                freq = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            _filter = new Filters.Butterworth.BandPassFilter(freq, 0.4, order);

            filterParamsDataGrid.RowCount = 2;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
            filterParamsDataGrid.Rows[1].Cells[1].Value = freq;
        }

        private void AnalyzeEllipticFilter()
        {
            var order = 4;
            var freq = 0.15;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                freq = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            // example how to convert linear scale specifications to decibel scale:

            var deltaPass = 0.96;
            var deltaStop = 0.04;

            var ripplePassDb = Utils.Scale.ToDecibel(1 / deltaPass);
            var attenuateDb = Utils.Scale.ToDecibel(1 / deltaStop);

            _filter = new Filters.Elliptic.LowPassFilter(freq, order, ripplePassDb, attenuateDb);

            filterParamsDataGrid.RowCount = 2;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
            filterParamsDataGrid.Rows[1].Cells[1].Value = freq;
        }

        private void AnalyzeChebyshevIFilter()
        {
            var order = 6;
            var freq = 0.2;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                freq = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            _filter = new Filters.ChebyshevI.HighPassFilter(freq, order);

            filterParamsDataGrid.RowCount = 2;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
            filterParamsDataGrid.Rows[1].Cells[1].Value = freq;
        }

        private void AnalyzeChebyshevIIFilter()
        {
            var order = 4;
            var freq = 0.25;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                freq = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            _filter = new Filters.ChebyshevII.BandStopFilter(freq, 0.4, order);

            filterParamsDataGrid.RowCount = 2;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
            filterParamsDataGrid.Rows[1].Cells[1].Value = freq;
        }

        private void AnalyzeBesselFilter()
        {
            var order = 4;
            var freq = 0.15;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                freq = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            _filter = new Filters.Bessel.LowPassFilter(freq, order);

            filterParamsDataGrid.RowCount = 2;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
            filterParamsDataGrid.Rows[1].Cells[1].Value = freq;
        }

        private void AnalyzeThiranFilter()
        {
            var order = 10;
            var delta = 10.3;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                delta = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            _filter = new ThiranFilter(order, order + delta);

            filterParamsDataGrid.RowCount = 2;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "delta";
            filterParamsDataGrid.Rows[1].Cells[1].Value = delta;
        }

        private void AnalyzeEquirippleLpFilter()
        {
            var order = 47;
            var fp = 0.15;
            var fa = 0.18;
            var ripplePass = 1.0;   // dB
            var rippleStop = 42.0;  // dB

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                fp = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
                fa = Convert.ToDouble(filterParamsDataGrid.Rows[2].Cells[1].Value);
                ripplePass = Convert.ToDouble(filterParamsDataGrid.Rows[3].Cells[1].Value);
                rippleStop = Convert.ToDouble(filterParamsDataGrid.Rows[4].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            var wp = Remez.DbToPassbandWeight(ripplePass);
            var wa = Remez.DbToStopbandWeight(rippleStop);

            _filter = new FirFilter(DesignFilter.FirEquirippleLp(order, fp, fa, wp, wa));

            filterParamsDataGrid.RowCount = 5;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "fp";
            filterParamsDataGrid.Rows[1].Cells[1].Value = fp;
            filterParamsDataGrid.Rows[2].Cells[0].Value = "fa";
            filterParamsDataGrid.Rows[2].Cells[1].Value = fa;
            filterParamsDataGrid.Rows[3].Cells[0].Value = "rp";
            filterParamsDataGrid.Rows[3].Cells[1].Value = ripplePass;
            filterParamsDataGrid.Rows[4].Cells[0].Value = "rs";
            filterParamsDataGrid.Rows[4].Cells[1].Value = rippleStop;
        }

        private void AnalyzeEquirippleBsFilter()
        {
            var order = 51;
            var fp1 = 0.19;
            var fa1 = 0.21;
            var fa2 = 0.39;
            var fp2 = 0.41;
            var ripplePass1 = 1.0;
            var rippleStop = 24.0;
            var ripplePass2 = 3.0;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                fp1 = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
                fa1 = Convert.ToDouble(filterParamsDataGrid.Rows[2].Cells[1].Value);
                fa2 = Convert.ToDouble(filterParamsDataGrid.Rows[3].Cells[1].Value);
                fp2 = Convert.ToDouble(filterParamsDataGrid.Rows[4].Cells[1].Value);
                ripplePass1 = Convert.ToDouble(filterParamsDataGrid.Rows[5].Cells[1].Value);
                rippleStop = Convert.ToDouble(filterParamsDataGrid.Rows[6].Cells[1].Value);
                ripplePass2 = Convert.ToDouble(filterParamsDataGrid.Rows[7].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            var freqs = new[] { 0, fp1, fa1, fa2, fp2, 0.5 };

            var weights = new[]
            {
                Remez.DbToPassbandWeight(ripplePass1),
                Remez.DbToStopbandWeight(rippleStop),
                Remez.DbToPassbandWeight(ripplePass2),
            };

            var remez = new Remez(order, freqs, new double[] { 1, 0, 1 }, weights);

            _filter = new FirFilter(remez.Design());

            var extrema = string.Join("\t", Enumerable.Range(0, remez.K).Select(e => remez.ExtremalFrequencies[e].ToString("F5")));
            var message = $"Iterations: {remez.Iterations}\n\nEstimated order: {Remez.EstimateOrder(freqs, weights)}\n\nExtrema:\n{extrema}";
            MessageBox.Show(message);

            filterParamsDataGrid.RowCount = 8;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "fp1";
            filterParamsDataGrid.Rows[1].Cells[1].Value = fp1;
            filterParamsDataGrid.Rows[2].Cells[0].Value = "fa1";
            filterParamsDataGrid.Rows[2].Cells[1].Value = fa1;
            filterParamsDataGrid.Rows[3].Cells[0].Value = "fa2";
            filterParamsDataGrid.Rows[3].Cells[1].Value = fa2;
            filterParamsDataGrid.Rows[4].Cells[0].Value = "fp2";
            filterParamsDataGrid.Rows[4].Cells[1].Value = fp2;
            filterParamsDataGrid.Rows[5].Cells[0].Value = "rp1";
            filterParamsDataGrid.Rows[5].Cells[1].Value = ripplePass1;
            filterParamsDataGrid.Rows[6].Cells[0].Value = "rs";
            filterParamsDataGrid.Rows[6].Cells[1].Value = rippleStop;
            filterParamsDataGrid.Rows[7].Cells[0].Value = "rp2";
            filterParamsDataGrid.Rows[7].Cells[1].Value = ripplePass2;
        }

        private void AnalyzeCustomLpFilter()
        {
            var order = 23;
            var freq = 0.22;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                freq = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            _filter = new FirFilter(DesignFilter.FirWinLp(order, freq));

            // for double precision and FDA:

            //var tf = new TransferFunction(DesignFilter.FirWinLp(order, freq));
            //_filter = new FirFilter(tf);

            filterParamsDataGrid.RowCount = 2;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "freq";
            filterParamsDataGrid.Rows[1].Cells[1].Value = freq;
        }

        private void AnalyzeCustomBandpassFilter()
        {
            var order = 231;
            var freq1 = 0.06;
            var freq2 = 0.2;

            if (filterParamsDataGrid.RowCount > 0)
            {
                order = Convert.ToInt32(filterParamsDataGrid.Rows[0].Cells[1].Value);
                freq1 = Convert.ToDouble(filterParamsDataGrid.Rows[1].Cells[1].Value);
                freq2 = Convert.ToDouble(filterParamsDataGrid.Rows[2].Cells[1].Value);
            }

            orderNumeratorTextBox.Text = (order - 1).ToString();
            orderDenominatorTextBox.Text = (order - 1).ToString();

            //_filter = new FirFilter(DesignFilter.FirWinBp(order, freq1, freq2));

            // for double precision and FDA:

            var tf = new TransferFunction(DesignFilter.FirWinBp(order, freq1, freq2));
            _filter = new FirFilter(tf);

            filterParamsDataGrid.RowCount = 3;
            filterParamsDataGrid.Rows[0].Cells[0].Value = "order";
            filterParamsDataGrid.Rows[0].Cells[1].Value = order;
            filterParamsDataGrid.Rows[1].Cells[0].Value = "freq1";
            filterParamsDataGrid.Rows[1].Cells[1].Value = freq1;
            filterParamsDataGrid.Rows[2].Cells[0].Value = "freq2";
            filterParamsDataGrid.Rows[2].Cells[1].Value = freq2;
        }

        #endregion

        #region filtering

        private void autoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void overlapAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringMethod.OverlapAdd);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void overlapSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringMethod.OverlapSave);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void differenceEquationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringMethod.DifferenceEquation);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
        }

        private void framebyFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if DEBUG
            if (_signal == null) return;

            _filter.Reset();

            _filteredSignal = _filter.ProcessChunks(_signal);
            //_filteredSignal = _filter.ProcessChunks(_signal, method: FilteringMethod.OverlapAdd);
            signalAfterFilteringPanel.Signal = _filteredSignal;
            spectrogramAfterFilteringPanel.Spectrogram = _stft.Spectrogram(_filteredSignal);
#endif
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
                var waveFile = new WaveFile(stream);
                _bitDepth = waveFile.WaveFmt.BitsPerSample;
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
                var waveFile = new WaveFile(_filteredSignal, _bitDepth);
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
            await _player.PlayAsync(_filteredSignal, _bitDepth);
        }

#endregion
    }
}
