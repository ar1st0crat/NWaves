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
        /// Online filtering
        /// </summary>
        /// <param name="input">Input block of samples</param>
        /// <param name="output">Block of filtered samples</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        /// <param name="method">General filtering strategy</param>
        public override void Process(float[] input,
                                     float[] output,
                                     int count,
                                     int inputPos = 0,
                                     int outputPos = 0,
                                     FilteringMethod method = FilteringMethod.Auto)
        {
            var b = _b32;
            var a = _a32;

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                output[m] = b[0] * input[n] + b[1] * _in1 - a[1] * _out1;
                _in1 = input[n];
                _out1 = output[m];
            }
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
