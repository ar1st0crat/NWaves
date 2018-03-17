using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NWaves.DemoForms.UserControls
{
    public partial class GroupPlot : UserControl
    {
        /// <summary>
        /// Groups to plot
        /// </summary>
        private float[][] _groups;
        public float[][] Groups
        {
            get { return _groups; }
            set
            {
                _groups = value;
                if (_groups == null) return;
                AutoScrollMinSize = new Size(_groups.Max(g => g.Length) * Stride + 20, 0);
                MakeBitmap();
                Invalidate();
            }
        }

        public int Stride { get; set; } = 2;
        public int Gain { get; set; } = 100;


        public GroupPlot()
        {
            InitializeComponent();
        }

        private void GroupPlot_Load(object sender, EventArgs e)
        {
            MakeBitmap();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(_bmp, 0, 0,
                new Rectangle(-AutoScrollPosition.X, 0, Width, Height),
                GraphicsUnit.Pixel);
        }

        private Bitmap _bmp;

        private void MakeBitmap()
        {
            var width = Math.Max(AutoScrollMinSize.Width, Width);

            _bmp = new Bitmap(width, Height);

            var g = Graphics.FromImage(_bmp);
            g.Clear(Color.White);

            var offset = Height - 30;

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
                        20 + x - Stride, -_groups[j][i - 1] * Gain + offset,
                        20 + x,          -_groups[j][i]     * Gain + offset);
                    x += Stride;
                    i++;
                }

                pen.Dispose();
            }

            g.Dispose();
        }
    }
}
