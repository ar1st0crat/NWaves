using System;
using System.Drawing;
using System.Windows.Forms;
using NWaves.Signals;

namespace NWaves.DemoForms.UserControls
{
    public partial class SignalPlot : UserControl
    {
        /// <summary>
        /// Signal to plot
        /// </summary>
        private DiscreteSignal _signal;
        public DiscreteSignal Signal
        {
            get { return _signal; }
            set
            {
                _signal = value;
                if (_signal == null) return;
                AutoScrollMinSize = new Size(_signal.Length / _stride + 20, 0);
                MakeBitmap();
                Invalidate();
            }
        }

        private int _stride = 64;
        public int Stride
        {
            get { return _stride; }
            set
            {
                _stride = value > 1 ? value : 1;
                if (_signal == null) return;
                AutoScrollMinSize = new Size(_signal.Length / _stride + 20, 0);
                MakeBitmap();
                Invalidate();
            }
        }

        public double Gain { get; set; } = 1;


        public SignalPlot()
        {
            InitializeComponent();
            ForeColor = Color.Blue;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_bmp == null) MakeBitmap();

            e.Graphics.DrawImage(_bmp, 0, 0, 
                new Rectangle(-AutoScrollPosition.X, 0, Width, Height),
                GraphicsUnit.Pixel);
        }

        private void buttonZoomIn_Click(object sender, System.EventArgs e)
        {
            if (_stride < 4)
            {
                Stride++;
            }
            else
            {
                Stride = (int)(_stride * 1.25);
            }
        }

        private void buttonZoomOut_Click(object sender, System.EventArgs e)
        {
            Stride = (int)(_stride / 1.25);
        }

        private Bitmap _bmp;

        private void MakeBitmap()
        {
            var width = Math.Max(AutoScrollMinSize.Width, Width);

            _bmp = new Bitmap(width, Height);

            var g = Graphics.FromImage(_bmp);
            g.Clear(Color.White);

            var offset = Height / 2;

            var gray = new Pen(Color.LightGray) { DashPattern = new[] { 2f, 2f } };

            for (var k = 0; k < offset; k += 10)
            {
                g.DrawLine(gray, 0, offset + k, width, offset + k);
                g.DrawLine(gray, 0, offset - k, width, offset - k);
            }

            gray.Dispose();

            var black = new Pen(Color.Black);

            g.DrawLine(black, 20, offset, width, offset);
            g.DrawLine(black, 20, 5, 20, Height - 5);

            black.Dispose();

            if (_signal != null)
            {
                var pen = new Pen(ForeColor);

                var i = 0;
                var x = 20;

                while (i < _signal.Length - _stride)
                {
                    var j = 0;
                    var min = 0.0;
                    var max = 0.0;
                    while (j < _stride)
                    {
                        if (_signal[i + j] > max) max = _signal[i + j];
                        if (_signal[i + j] < min) min = _signal[i + j];
                        j++;
                    }
                    g.DrawLine(pen, x, (float) (-min*Gain) + offset, x, (float) (-max*Gain) + offset);
                    x++;
                    i += _stride;
                }

                pen.Dispose();
            }

            g.Dispose();
        }
    }
}
