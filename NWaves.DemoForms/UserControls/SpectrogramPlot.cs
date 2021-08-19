using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SciColorMaps;

namespace NWaves.DemoForms.UserControls
{
    public partial class SpectrogramPlot : UserControl
    {
        private List<float[]> _spectrogram;
        public List<float[]> Spectrogram
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
                        var sqrt = (float)Math.Sqrt(s);
                        return sqrt*3 < maxValue ? sqrt*3 : sqrt/1.5f;
                    })
                    .ToArray();
                }
                maxValue /= 12;

                _cmap = new ColorMap(ColorMapName, minValue, maxValue);

                AutoScrollMinSize = new Size(_spectrogram.Count, 0);

                Invalidate();
            }
        }

        private float[] _markline;
        public float[] Markline
        {
            get { return _markline; }
            set
            {
                _markline = value;
                Invalidate();
            }
        }

        private int _marklineThickness;
        public int MarklineThickness
        {
            get { return _marklineThickness; }
            set
            {
                _marklineThickness = value;
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

            var width = Math.Min(Width, _spectrogram.Count);
            var spectrogramBitmap = new Bitmap(width, _spectrogram[0].Length);

            var realPos = 0;
            var startPos = -AutoScrollPosition.X;
            for (var i = startPos; i < startPos + spectrogramBitmap.Width; i++, realPos++)
            {
                for (var j = 0; j < _spectrogram[i].Length; j++)
                {
                    spectrogramBitmap.SetPixel(realPos, _spectrogram[i].Length - 1 - j, _cmap.GetColor(_spectrogram[i][j]));
                }
            }

            g.DrawImage(spectrogramBitmap, 0, 0);

            if (_markline != null)
            {
                var pen = new Pen(Color.DeepPink, _marklineThickness);

                realPos = 1;
                for (var i = startPos + 1; i < Math.Min(startPos + spectrogramBitmap.Width, _markline.Length); i++, realPos++)
                {
                    g.DrawLine(pen, realPos - 1, _spectrogram[i].Length - 1 - (int) (_markline[i - 1]),
                        realPos, _spectrogram[i].Length - 1 - (int) (_markline[i]));
                }

                pen.Dispose();
            }
        }
    }
}
