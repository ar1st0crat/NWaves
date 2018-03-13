using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LevelScale = NWaves.Utils.Scale;

namespace NWaves.DemoForms.UserControls
{
    public partial class LinePlot : UserControl
    {
        /// <summary>
        /// Line to plot
        /// </summary>
        private double[] _line;
        public double[] Line
        {
            get { return _line; }
            set
            {
                _line = value;
                _logLine = null;
                if (_line == null) return;
                AutoScrollMinSize = new Size(_line.Length * Stride + 20, 0);
                MakeBitmap();
                Invalidate();
            }
        }

        /// <summary>
        /// Some mark line to plot
        /// </summary>
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

        private int? _mark;
        public int? Mark
        {
            get { return _mark; }
            set
            {
                _mark = value;
                Invalidate();
            }
        }
        
        public double Gain { get; set; } = 1;
        public int Thickness { get; set; } = 1;
        public int Stride { get; set; } = 1;
        public string Legend { get; set; }

        private double[] _logLine;


        public LinePlot()
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

        public void ToDecibel()
        {
            if (_line == null)
            {
                return;
            }

            if (_logLine == null)
            {
                _logLine = _line.Select(l =>
                {
                    var val = LevelScale.ToDecibel(l);
                    if (double.IsNaN(val)) val = Height / 2 + 1;
                    return val / Gain;
                })
                .ToArray();
            }
            else
            {
                _logLine = null;
            }

            MakeBitmap();
            Invalidate();
        }

        private void LinePlot_MouseClick(object sender, MouseEventArgs e)
        {
            ToDecibel();
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

            if (_line != null)
            {
                var pen = new Pen(ForeColor, Thickness);

                var i = 1;
                var x = 20 + Stride;

                var line = _logLine ?? _line;

                for (; i < line.Length; i++)
                {
                    g.DrawLine(pen, x - Stride, (float) (-line[i - 1]*Gain) + offset, 
                                    x,          (float) (-line[i]*Gain) + offset);
                    x += Stride;
                }

                pen.Dispose();
            }

            if (_logLine != null)
            {
                g.DrawString("(log)", new Font("arial", 12), new SolidBrush(ForeColor), Width - 50, 5);
            }


            if (_markline != null)
            {
                var pen = new Pen(Color.Red, 2);
                var x = Stride;
                for (var j = 1; j < _markline.Length; j++)
                {
                    g.DrawLine(pen, 20 + x - Stride, (float)(-_markline[j - 1] * Gain) + offset, 
                                    20 + x,          (float)(-_markline[j] * Gain) + offset);
                    x += Stride;
                }

                pen.Dispose();
            }

            var red = new Pen(Color.Red, 2);

            if (_mark != null)
            {
                g.DrawLine(red, 20 + _mark.Value * Stride, 20, 20 + _mark.Value * Stride, Height - 20);
            }

            if (Legend != null)
            {
                g.DrawString(Legend, new Font("arial", 16), new SolidBrush(Color.Red), 100, 30);
            }

            red.Dispose();

            g.Dispose();
        }
    }
}
