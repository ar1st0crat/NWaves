using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// DC removal IIR filter
    /// </summary>
    public class DcRemovalFilter : IirFilter
    {
        /// <summary>
        /// Delay line
        /// </summary>
        private float _in1;
        private float _out1;

        /// <summary>
        /// Constructor creates simple 1st order recursive filter
        /// </summary>
        /// <param name="r">R coefficient (usually in [0.9, 1] range)</param>
        public DcRemovalFilter(double r = 0.995) : base(new [] {1, -1.0}, new [] {1, -r})
        {
        }

        /// <summary>
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = _b32[0] * sample + _b32[1] * _in1 - _a32[1] * _out1;
            _in1 = sample;
            _out1 = output;

            return output;
        }

        /// <summary>
        /// Reset
        /// </summary>
        public override void Reset()
        {
            _in1 = _out1 = 0;
        }
    }
}
