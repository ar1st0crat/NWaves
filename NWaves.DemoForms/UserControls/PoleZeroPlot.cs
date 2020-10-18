using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using NWaves.Signals;

namespace NWaves.DemoForms.UserControls
{
    public partial class PoleZeroPlot : UserControl
    {
        /// <summary>
        /// Poles
        /// </summary>
        private Complex[] _poles;
        public Complex[] Poles
        {
            get => _poles;
            set
            {
                _poles = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Zeros
        /// </summary>
        private Complex[] _zeros;
        public Complex[] Zeros
        {
            get => _zeros;
            set
            {
                _zeros = value;
                Invalidate();
            }
        }


        public PoleZeroPlot()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            var g = e.Graphics;
            g.Clear(Color.White);

            var offset = Height / 2;

            var gray = new Pen(Color.LightGray) { DashPattern = new[] { 2f, 2f } };

            for (var k = 0; k < offset; k += 10)
            {
                g.DrawLine(gray, 0, offset + k, Width, offset + k);
                g.DrawLine(gray, 0, offset - k, Width, offset - k);
            }

            gray.Dispose();


            var unitRadius = Height / 3;

            var cx = Width / 2;
            var cy = Height / 2;

            var pen = new Pen(Color.Blue);

            g.DrawLine(pen, 10, cy, Width - 10, cy);
            g.DrawLine(pen, cx, 10, cx, Height - 10);

            for (var i = 0; i < 360; i++)
            {
                var x = cx + unitRadius * Math.Cos(i * Math.PI / 180);
                var y = cy + unitRadius * Math.Sin(i * Math.PI / 180);

                g.DrawEllipse(pen, (int)x - 1, (int)y - 1, 1, 1);
            }

            pen.Dispose();

            var red = new Pen(Color.Red, 3);

            if (_zeros == null)
            {
                return;
            }

            for (var i = 0; i < _zeros.Length; i++)
            {
                var x = cx + unitRadius * _zeros[i].Real;
                var y = cy + unitRadius * _zeros[i].Imaginary;
                if (x - 4 > 0 && x + 4 < Width &&
                    y - 4 > 0 && y + 4 < Height)
                {
                    g.DrawEllipse(red, (int)x - 4, (int)y - 4, 8, 8);
                }
            }

            if (_poles == null)
            {
                return;
            }

            for (var i = 0; i < _poles.Length; i++)
            {
                var x = cx + unitRadius * _poles[i].Real;
                var y = cy + unitRadius * _poles[i].Imaginary;
                if (x - 6 > 0 && x + 6 < Width &&
                    y - 6 > 0 && y + 6 < Height)
                {
                    g.DrawLine(red, (int)x - 6, (int)y - 6, (int)x + 6, (int)y + 6);
                    g.DrawLine(red, (int)x + 6, (int)y - 6, (int)x - 6, (int)y + 6);
                }
            }

            red.Dispose();
        }
    }
}
