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
        /// Online filtering buffer-by-buffer
        /// </summary>
        /// <param name="input">Input buffer</param>
        /// <param name="filteringOptions">Ignored by BiQuad filters</param>
        /// <returns></returns>
        public override float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var output = new float[input.Length];

            var a = _a32;
            var b = _b32;

            for (var n = 0; n < input.Length; n++)
            {
                output[n] = b[0] * input[n] + b[1] * _in1 + b[2] * _in2 - a[1] * _out1 - a[2] * _out2;

                _in2 = _in1;
                _in1 = input[n];
                _out2 = _out1;
                _out1 = output[n];
            }

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
