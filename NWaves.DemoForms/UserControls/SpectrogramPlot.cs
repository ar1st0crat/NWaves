using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using SciColorMaps;

namespace NWaves.DemoForms.UserControls
{
    public partial class SpectrogramPlot : UserControl
    {
        private List<double[]> _spectrogram;
        public List<double[]> Spectrogram
        {
            get { return _spectrogram; }
            set
            {
                _spectrogram = value;

                if (_spectrogram == null)
                {
                    return;
                }

                // post-process spectrogram for better visualization

                var spectraCount = _spectrogram.Count;

                var minValue = _spectrogram.SelectMany(s => s).Min();
                var maxValue = _spectrogram.SelectMany(s => s).Max();

                for (var i = 0; i < spectraCount; i++)
                {
                    _spectrogram[i] = _spectrogram[i].Select(s =>
                    {
                        var sqrt = Math.Sqrt(s);
                        return sqrt*3 < maxValue ? sqrt*3 : sqrt/1.5;
                    })
                    .ToArray();
                }
                maxValue /= 12;

                _cmap = new ColorMap(ColorMapName, minValue, maxValue);

                AutoScrollMinSize = new Size(_spectrogram.Count, 0);

                Invalidate();
            }
        }

        private double[] _markline;
        public double[] Markline
        {
            get { return _markline; }
            set
            {
                _markline = value;
                Invalidate();
            }
        }

        public string ColorMapName { get; set; } = "magma";

        private ColorMap _cmap;


        public SpectrogramPlot()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_spectrogram == null)
            {
                return;
            }

            var g = e.Graphics;
            g.Clear(Color.White);

            var mx = new Matrix(1, 0, 0, 1, AutoScrollPosition.X, AutoScrollPosition.Y);
            g.Transform = mx;

            var spectrogramBitmap = new Bitmap(_spectrogram.Count, _spectrogram[0].Length);

            for (var i = 0; i < _spectrogram.Count; i++)
            {
                for (var j = 0; j < _spectrogram[i].Length; j++)
                {
                    spectrogramBitmap.SetPixel(i, _spectrogram[i].Length - 1 - j, _cmap.GetColor(_spectrogram[i][j]));
                }
            }

            g.DrawImage(spectrogramBitmap, 0, 0);

            if (_markline == null)
            {
                return;
            }

            var pen = new Pen(Color.DeepPink, 7);

            for (var i = 1; i < _markline.Length; i++)
            {
                g.DrawLine(pen, i - 1, _spectrogram[i].Length - 1 - (int)(_markline[i - 1]), 
                                i,     _spectrogram[i].Length - 1 - (int)(_markline[i]));
            }

            pen.Dispose();
        }
    }
}
