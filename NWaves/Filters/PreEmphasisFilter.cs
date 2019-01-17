using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Standard pre-emphasis FIR filter
    /// </summary>
    public class PreEmphasisFilter : FirFilter
    {
        /// <summary>
        /// Delay line
        /// </summary>
        private float _prev;

        /// <summary>
        /// Constructor computes simple 1st order kernel
        /// </summary>
        /// <param name="a">Pre-emphasis coefficient</param>
        public PreEmphasisFilter(double a = 0.97) : base(new [] {1, -a})
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
            var b = _kernel32;

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                output[m] = b[0] * input[n] + b[1] * _prev;
                _prev = input[n];
            }
        }

        /// <summary>
        /// Reset
        /// </summary>
        public override void Reset()
        {
            _prev = 0;
        }
    }
}
