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
        /// Online filtering
        /// </summary>
        /// <param name="input">Input block of samples</param>
        /// <param name="output">Block of filtered samples</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        /// <param name="method">Ignored by BiQuad filters</param>
        public override void Process(float[] input,
                                     float[] output,
                                     int count,
                                     int inputPos = 0,
                                     int outputPos = 0,
                                     FilteringMethod method = FilteringMethod.Auto)
        {
            var a = _a32;
            var b = _b32;

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                output[m] = b[0] * input[n] + b[1] * _in1 + b[2] * _in2 - a[1] * _out1 - a[2] * _out2;

                _in2 = _in1;
                _in1 = input[n];
                _out2 = _out1;
                _out1 = output[m];
            }
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
