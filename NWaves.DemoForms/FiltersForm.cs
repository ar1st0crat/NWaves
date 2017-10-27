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
using NWaves.Transforms;

namespace NWaves.DemoForms
{
    public partial class FiltersForm : Form
    {
        private LtiFilter _filter;

        private DiscreteSignal _signal;
        private List<double[]> _spectrogram;

        private DiscreteSignal _filteredSignal;
        private List<double[]> _filteredSpectrogram;

        private string _waveFileName;

        private readonly MciAudioPlayer _player = new MciAudioPlayer();

        public FiltersForm()
        {
            InitializeComponent();
        }

        private void buttonAnalyzeFilter_Click(object sender, EventArgs e)
        {
            switch (filterTypesComboBox.Text)
            {
                case "Custom IIR":
                    _filter = new IirFilter(new[] { 1, 0.4 }, new[] { 1, -0.2, 0.6 });
                    numeratorListBox.DataSource = (_filter as IirFilter).B;
                    denominatorListBox.DataSource = (_filter as IirFilter).A;
                    filterParamsDataGrid.RowCount = 4;
                    filterParamsDataGrid.Rows[0].Cells[0].Value = "b0";
                    filterParamsDataGrid.Rows[0].Cells[1].Value = "1";
                    filterParamsDataGrid.Rows[1].Cells[0].Value = "b1";
                    filterParamsDataGrid.Rows[1].Cells[1].Value = "0.4";
                    filterParamsDataGrid.Rows[2].Cells[0].Value = "a1";
                    filterParamsDataGrid.Rows[2].Cells[1].Value = "-0.6";
                    filterParamsDataGrid.Rows[3].Cells[0].Value = "a2";
                    filterParamsDataGrid.Rows[3].Cells[1].Value = "0.2";
                    break;
                case "Custom FIR":
                    _filter = new FirFilter(new[] {1, 0.1, 0.7});
                    numeratorListBox.DataSource = (_filter as FirFilter).Kernel;
                    denominatorListBox.DataSource = new[] { 1.0 };
                    filterParamsDataGrid.RowCount = 3;
                    filterParamsDataGrid.Rows[0].Cells[0].Value = "b0";
                    filterParamsDataGrid.Rows[0].Cells[1].Value = "1";
                    filterParamsDataGrid.Rows[1].Cells[0].Value = "b1";
                    filterParamsDataGrid.Rows[1].Cells[1].Value = "0.5";
                    filterParamsDataGrid.Rows[2].Cells[0].Value = "b2";
                    filterParamsDataGrid.Rows[2].Cells[1].Value = "-0.7";
                    break;
                case "BiQuad":
                    _filter = new BiQuadFilter(freq: 0.2, width: 0.3);
                    break;
                case "Moving average":
                    _filter = new MovingAverageFilter(5);
                    numeratorListBox.DataSource = (_filter as FirFilter).Kernel;
                    denominatorListBox.DataSource = new[] { 1.0 };
                    filterParamsDataGrid.RowCount = 1;
                    filterParamsDataGrid.Rows[0].Cells[0].Value = "size";
                    filterParamsDataGrid.Rows[0].Cells[1].Value = "5";
                    break;
                case "Moving average recursive":
                    _filter = new MovingAverageRecursiveFilter(5);
                    numeratorListBox.DataSource = (_filter as IirFilter).B;
                    denominatorListBox.DataSource = (_filter as IirFilter).A;
                    filterParamsDataGrid.RowCount = 1;
                    filterParamsDataGrid.Rows[0].Cells[0].Value = "size";
                    filterParamsDataGrid.Rows[0].Cells[1].Value = "5";
                    break;
                case "Pre-emphasis":
                    _filter = new PreEmphasisFilter(0.95);
                    numeratorListBox.DataSource = (_filter as FirFilter).Kernel;
                    denominatorListBox.DataSource = new[] { 1.0 };
                    filterParamsDataGrid.RowCount = 1;
                    filterParamsDataGrid.Rows[0].Cells[0].Value = "a";
                    filterParamsDataGrid.Rows[0].Cells[1].Value = "0.95";
                    break;
            }

            DrawFrequencyResponse();
            DrawPoleZeroPlot();
        }
        
        private void overlapAddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringOptions.OverlapAdd);
            DrawSignal(signalAfterFilteringPanel, _filteredSignal);

            _filteredSpectrogram = Transform.Stft(_filteredSignal.Samples);
            DrawSpectrogram(spectrogramAfterFilteringPanel, _filteredSpectrogram);
        }

        private void overlapSaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringOptions.OverlapSave);
            DrawSignal(signalAfterFilteringPanel, _filteredSignal);

            _filteredSpectrogram = Transform.Stft(_filteredSignal.Samples);
            DrawSpectrogram(spectrogramAfterFilteringPanel, _filteredSpectrogram);
        }

        private void differenceEquationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signal == null) return;

            _filteredSignal = _filter.ApplyTo(_signal, FilteringOptions.DifferenceEquation);
            DrawSignal(signalAfterFilteringPanel, _filteredSignal);

            _filteredSpectrogram = Transform.Stft(_filteredSignal.Samples);
            DrawSpectrogram(spectrogramAfterFilteringPanel, _filteredSpectrogram);
        }


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

            DrawSignal(signalBeforeFilteringPanel, _signal);

            _spectrogram = Transform.Stft(_signal.Samples);
            DrawSpectrogram(spectrogramBeforeFilteringPanel, _spectrogram);
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

        #region drawing

        private void DrawSignal(Control panel, DiscreteSignal signal, int step = 256)
        {
            var g = panel.CreateGraphics();
            g.Clear(Color.White);

            var offset = panel.Height / 2;

            var pen = panel == signalBeforeFilteringPanel ? new Pen(Color.Blue) : new Pen(Color.Red);

            var i = 0;
            var x = 0;

            while (i < signal.Samples.Length)
            {
                if (Math.Abs(signal[i] * 160) < panel.Height)
                {
                    g.DrawLine(pen, x, offset, x, (float)-signal[i] * 160 + offset);
                    g.DrawEllipse(pen, x - 1, (int)(-signal[i] * 160) + offset - 1, 3, 3);
                }
                x++;
                i += step;

            }

            pen.Dispose();
        }

        private void DrawPoleZeroPlot()
        {
            var g = poleZeroPanel.CreateGraphics();
            g.Clear(Color.White);

            const int unitRadius = 80;

            var cx = poleZeroPanel.Width / 2;
            var cy = poleZeroPanel.Height / 2;

            var pen = new Pen(Color.Blue);

            g.DrawLine(pen, 10, cy, poleZeroPanel.Width - 10, cy);
            g.DrawLine(pen, cx, 10, cx, poleZeroPanel.Height - 10);

            for (var i = 0; i < 360; i++)
            {
                var x = cx + unitRadius * Math.Cos(i * Math.PI / 180);
                var y = cy + unitRadius * Math.Sin(i * Math.PI / 180);

                g.DrawEllipse(pen, (int)x - 1, (int)y - 1, 1, 1);
            }

            pen.Dispose();

            var redPen = new Pen(Color.Red, 3);

            var zeros = _filter.Zeros;
            if (zeros == null)
            {
                return;
            }

            for (var i = 0; i < zeros.Real.Length; i++)
            {
                var x = cx + unitRadius * zeros.Real[i];
                var y = cy + unitRadius * zeros.Imag[i];
                g.DrawEllipse(redPen, (int)x - 4, (int)y - 4, 8, 8);
            }

            var poles = _filter.Poles;
            if (poles == null)
            {
                return;
            }

            for (var i = 0; i < poles.Real.Length; i++)
            {
                var x = cx + unitRadius * poles.Real[i];
                var y = cy + unitRadius * poles.Imag[i];
                g.DrawLine(redPen, (int)x - 4, (int)y - 4, (int)x + 4, (int)y + 4);
                g.DrawLine(redPen, (int)x + 4, (int)y - 4, (int)x - 4, (int)y + 4);
            }

            redPen.Dispose();
        }
        
        private void DrawSpectrogram(Control panel, List<double[]> spectrogram)
        {
            var g = panel.CreateGraphics();
            g.Clear(Color.White);

            var spectrogramBitmap = new Bitmap(spectrogram.Count, spectrogram[0].Length);

            for (var i = 0; i < spectrogram.Count; i++)
            {
                for (var j = 0; j < spectrogram[i].Length; j++)
                {
                    spectrogramBitmap.SetPixel(i, spectrogram[i].Length - 1 - j, Color.FromArgb(0, (byte)(spectrogram[i][j] * 200), 0));
                }
            }

            g.DrawImage(spectrogramBitmap, 0, 0);
        }

        private void DrawFrequencyResponse(int step = 2)
        {
            var g = magnitudeResponsePanel.CreateGraphics();
            g.Clear(Color.White);
            var pen = new Pen(Color.Blue);

            var offset = magnitudeResponsePanel.Height - 20;
            
            var magnitudeResponse = _filter.FrequencyResponse.Magnitude;

            var i = 0;
            var x = 0;
            while (i < magnitudeResponse.Samples.Length / 2)
            {
                if (Math.Abs(magnitudeResponse[i] * 40) < magnitudeResponsePanel.Height)
                {
                    g.DrawLine(pen, x, offset, x, (float)-magnitudeResponse[i] * 40 + offset);
                    g.DrawEllipse(pen, x - 1, (int)(-magnitudeResponse[i] * 40) + offset - 1, 3, 3);
                }
                x += step;
                i++;

            }

            g = phaseResponsePanel.CreateGraphics();
            g.Clear(Color.White);

            var phaseResponse = _filter.FrequencyResponse.Phase;

            offset = phaseResponsePanel.Height / 2;

            i = 0;
            x = 0;
            while (i < phaseResponse.Samples.Length / 2)
            {
                if (Math.Abs(phaseResponse[i] * 70) < magnitudeResponsePanel.Height)
                {
                    g.DrawLine(pen, x, offset, x, (float)-phaseResponse[i] * 50 + offset);
                    g.DrawEllipse(pen, x - 1, (int)(-phaseResponse[i] * 50) + offset - 1, 3, 3);
                }
                x += step;
                i++;
            }

            pen.Dispose();
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

/*
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
            */
