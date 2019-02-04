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
        /// Constructor for subclasses
        /// </summary>
        /// <param name="tf"></param>
        protected BiQuadFilter(TransferFunction tf) : base(tf)
        {
        }

        /// <summary>
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = _b32[0] * sample + _b32[1] * _in1 + _b32[2] * _in2 - _a32[1] * _out1 - _a32[2] * _out2;

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
    }
}
