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
using NWaves.Transforms;
using NWaves.Windows;
using SciColorMaps;

namespace NWaves.DemoForms
{
    public partial class StftForm : Form
    {
        private DiscreteSignal _signal;
        private DiscreteSignal _processedSignal;

        private Stft _stft;
        private List<double[]> _spectrogram;

        private string _waveFileName;

        private readonly IAudioPlayer _player = new MciAudioPlayer();


        public StftForm()
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

            _waveFileName = ofd.FileName;

            using (var stream = new FileStream(_waveFileName, FileMode.Open))
            {
                var waveFile = new WaveFile(stream);
                _signal = waveFile[Channels.Average];
            }

            _stft = new Stft(1024, 512);

            var processed = _stft.Inverse(_stft.Direct(_signal));
            _processedSignal = new DiscreteSignal(_signal.SamplingRate, processed);

            DrawSignal(signalPanel, _signal);
            DrawSignal(processedSignalPanel, _processedSignal);

            _spectrogram = _stft.Spectrogram(_signal);
            DrawSpectrogram(spectrogramPanel, _spectrogram);
        }

        private async void playToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string tmpFilename = "tmpfiltered.wav";
            using (var stream = new FileStream(tmpFilename, FileMode.Create))
            {
                var waveFile = new WaveFile(_processedSignal);
                waveFile.SaveTo(stream);
            }

            await _player.PlayAsync(tmpFilename);
        }


        #region drawing

        private void DrawSignal(Control panel, DiscreteSignal signal, int stride = 256)
        {
            var g = panel.CreateGraphics();
            g.Clear(Color.White);

            var offset = panel.Height / 2;

            var pen = panel == signalPanel ? new Pen(Color.Blue) : new Pen(Color.Red);

            var i = 0;
            var x = 0;

            while (i < signal.Length - stride)
            {
                var j = 0;
                var min = 0.0;
                var max = 0.0;
                while (j < stride)
                {
                    if (signal[i + j] > max) max = signal[i + j];
                    if (signal[i + j] < min) min = signal[i + j];
                    j++;
                }
                g.DrawLine(pen, x, (float)-min * 70 + offset, x, (float)-max * 70 + offset);
                x++;
                i += stride;

            }

            pen.Dispose();
        }

        private void DrawSpectrogram(Control panel, List<double[]> spectrogram)
        {
            var g = panel.CreateGraphics();
            g.Clear(Color.White);

            var spectraCount = spectrogram.Count;

            var minValue = spectrogram.SelectMany(s => s).Min();
            var maxValue = spectrogram.SelectMany(s => s).Max();

            // post-process spectrogram for better visualization
            for (var i = 0; i < spectraCount; i++)
            {
                spectrogram[i] = spectrogram[i].Select(s =>
                {
                    var sqrt = Math.Sqrt(s);
                    return sqrt * 3 < maxValue ? sqrt * 3 : sqrt / 1.5;
                })
                .ToArray();
            }
            maxValue /= 12;

            var cmap = new ColorMap("magma", minValue, maxValue);


            var spectrogramBitmap = new Bitmap(spectrogram.Count, spectrogram[0].Length);

            for (var i = 0; i < spectrogram.Count; i++)
            {
                for (var j = 0; j < spectrogram[i].Length; j++)
                {
                    spectrogramBitmap.SetPixel(i, spectrogram[i].Length - 1 - j, cmap.GetColor(spectrogram[i][j]));
                }
            }

            g.DrawImage(spectrogramBitmap, 0, 0);
        }

        #endregion
    }
}
