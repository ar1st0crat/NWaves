using NWaves.Filters.Base;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad filter base class
    /// </summary>
    public class BiQuadFilter : IirFilter
    {
        /// <summary>
        /// Delay line
        /// </summary>
        private float _in1;
        private float _in2;
        private float _out1;
        private float _out2;

        /// <summary>
        /// Constructor
        /// </summary>
        protected BiQuadFilter() : base(new[] { 1.0, 0, 0 }, new[] { 1.0, 0, 0 })
        {
        }

        /// <summary>
        /// Constructor for user-defined TF
        /// </summary>
        /// <param name="b0"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <param name="a0"></param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        public BiQuadFilter(double b0, double b1, double b2,
                            double a0, double a1, double a2) : 
            base(new[] { b0, b1, b2 }, new[] { a0, a1, a2 })
        {
        }

        /// <summary>
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = _b[0] * sample + _b[1] * _in1 + _b[2] * _in2 - _a[1] * _out1 - _a[2] * _out2;

            _in2 = _in1;
            _in1 = sample;
            _out2 = _out1;
            _out1 = output;

            return output;
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public override void Reset()
        {
            _in1 = _in2 = _out1 = _out2 = 0;
        }

        /// <summary>
        /// Change filter coefficients (preserving its state)
        /// </summary>
        /// <param name="b0"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <param name="a0"></param>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        public void Change(float b0, float b1, float b2, float a0, float a1, float a2)
        {
            _b[0] = b0;
            _b[1] = b1;
            _b[2] = b2;
            _a[0] = a0;
            _a[1] = a1;
            _a[2] = a2;
        }
    }
}
