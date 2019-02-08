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
        private float[] _line;
        public float[] Line
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
        private float[] _markline;
        public float[] Markline
        {
            get { return _markline; }
            set
            {
                _markline = value;
                _logMarkline = null;
                MakeBitmap();
                Invalidate();
            }
        }

        /// <summary>
        /// One vertical markline
        /// </summary>
        private int? _mark;
        public int? Mark
        {
            get { return _mark; }
            set
            {
                _mark = value;
                MakeBitmap();
                Invalidate();
            }
        }
        
        public float? Gain { get; set; } = null;
        public int Thickness { get; set; } = 1;
        public int Stride { get; set; } = 1;
        public string Legend { get; set; }

        public int PaddingX { get; set; } = 30;
        public int PaddingY { get; set; } = 20;

        private float[] _logLine;
        private float[] _logMarkline;


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
                    var val = (float)LevelScale.ToDecibel(l);
                    //if (float.IsNaN(val)) val = Height / 2 + 1;
                    if (float.IsNaN(val) || Math.Abs(val) > int.MaxValue) val = Height / 2 + 1;
                    return val / Gain ?? val;
                })
                .ToArray();
            }
            else
            {
                _logLine = null;
            }

            if (_markline != null)
            {
                if (_logMarkline == null)
                {
                    _logMarkline = _markline.Select(l =>
                    {
                        var val = (float)LevelScale.ToDecibel(l);
                        if (float.IsNaN(val) || Math.Abs(val) > Height) val = Height/2 + 1;
                        return val / Gain ?? val;
                    })
                        .ToArray();
                }
                else
                {
                    _logMarkline = null;
                }
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

            var min = 0.0f;
            var max = 0.0f;

            if (_line != null)
            {
                var pen = new Pen(ForeColor, Thickness);

                var x = PaddingX + Stride;

                var line = _logLine ?? _line;
                
                var gain = Gain;
                if (!Gain.HasValue)
                {
                    min = line.Min();
                    max = line.Max();

                    gain = max - min < 1e-6 ? 1 : (Height - 2*PaddingY) / (max - min);

                    offset = (int)(Height - PaddingY + min * gain);

                    DrawAxes(g, min, max);
                }
                else
                {
                    DrawAxes(g, -(Height - 2*PaddingY) / (2 * gain.Value), 
                                 (Height-2*PaddingY) / (2 * gain.Value));
                }
                
                for (var i = 1; i < line.Length; i++)
                {
                    g.DrawLine(pen, x - Stride, (float)(-line[i - 1] * gain) + offset, 
                                    x,          (float)(-line[i] * gain) + offset);
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
                var markline = _logMarkline ?? _markline;

                var gain = Gain;
                if (!Gain.HasValue)
                {
                    if (_line == null)
                    {
                        min = markline.Min();
                        max = markline.Max();

                        DrawAxes(g, min, max);
                    }

                    gain = max - min < 1e-6 ? 1 : (Height - 2 * PaddingY) / (max - min);

                    offset = (int)(Height - PaddingY + min * gain);
                }

                var pen = new Pen(Color.Red, 2);
                var x = PaddingX + Stride;
                for (var j = 1; j < _markline.Length; j++)
                {
                    var y1 = Math.Abs(markline[j - 1]) < Math.Abs(Height) ? (float)(-markline[j - 1] * gain) : 0;
                    var y2 = Math.Abs(markline[j]) < Math.Abs(Height) ? (float)(-markline[j] * gain) : 0;

                    g.DrawLine(pen, x - Stride, y1 + offset, x, y2 + offset);
                    x += Stride;
                }

                pen.Dispose();
            }

            var red = new Pen(Color.Red, 2);

            if (_mark != null)
            {
                g.DrawLine(red, PaddingX + _mark.Value * Stride, PaddingY, 
                                PaddingX + _mark.Value * Stride, Height - PaddingY);
            }

            if (Legend != null)
            {
                var font = new Font("arial", 16);
                var brush = new SolidBrush(Color.Red);
                g.DrawString(Legend, font, brush, 100, 30);
                font.Dispose();
                brush.Dispose();
            }

            red.Dispose();

            g.Dispose();
        }

        private void DrawAxes(Graphics g, float min, float max)
        {
            var black = new Pen(Color.Black);

            g.DrawLine(black, PaddingX, Height - PaddingY, _bmp.Width, Height - PaddingY);
            g.DrawLine(black, PaddingX, 10, PaddingX, Height - PaddingY);

            var font = new Font("arial", 5);
            var brush = new SolidBrush(Color.Black);

            const int stride = 20;
            var pos = Height - 12;
            var n = (Height - 2*PaddingY) / stride;
            for (var i = 0; i <= n; i++)
            {
                g.DrawString(string.Format("{0:F2}", min + i*(max-min)/n), font, brush, 1, pos -= stride);
            }

            font.Dispose();
            brush.Dispose();

            black.Dispose();
        }
    }
}
