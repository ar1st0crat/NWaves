using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NWaves.DemoForms.UserControls
{
    public partial class GroupPlot : UserControl
    {
        /// <summary>
        /// Groups to plot
        /// </summary>
        private double[][] _groups;
        public double[][] Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                if (_groups == null) return;
                AutoScrollMinSize = new Size(_groups.Length * Stride + 20, 0);
                Invalidate();
            }
        }

        public int Stride { get; set; } = 2;
        public int Gain { get; set; } = 100;


        public GroupPlot()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.Clear(Color.White);

            var mx = new Matrix(1, 0, 0, 1, AutoScrollPosition.X, AutoScrollPosition.Y);
            g.Transform = mx;

            var offset = Height - 20;

            var gray = new Pen(Color.LightGray) { DashPattern = new[] { 2f, 2f } };

            var width = Math.Max(_groups?.Length * Stride + 20 ?? Width, Width);

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

            if (_groups == null)
            {
                return;
            }


            var rand = new Random();
            
            for (var j = 0; j < _groups.Length; j++)
            {
                var pen = new Pen(Color.FromArgb(rand.Next() % 255, rand.Next() % 255, rand.Next() % 255));

                var i = 1;
                var x = Stride;

                while (i < _groups[j].Length)
                {
                    g.DrawLine(pen,
                                20 + x - Stride, (float)-_groups[j][i - 1] * Gain + offset,
                                20 + x,          (float)-_groups[j][i] * Gain + offset);
                    x += Stride;
                    i++;
                }

                pen.Dispose();
            }
        }
    }
}
