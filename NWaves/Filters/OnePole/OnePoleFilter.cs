using System.Collections.Generic;
using NWaves.Filters.Base;

namespace NWaves.Filters.OnePole
{
    /// <summary>
    /// One-Pole filter base class
    /// </summary>
    public class OnePoleFilter : IirFilter
    {
        /// <summary>
        /// Delay line
        /// </summary>
        private float _prev;

        /// <summary>
        /// Constructor for subclasses
        /// </summary>
        /// <param name="b"></param>
        /// <param name="a"></param>
        protected OnePoleFilter(IEnumerable<double> b, double a) : base(b, new [] { 1.0, a })
        {
        }

        /// <summary>
        /// Constructor for subclasses
        /// </summary>
        /// <param name="tf"></param>
        protected OnePoleFilter(TransferFunction tf) : base(tf)
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
            var a = _a32;
            var b = _b32;

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                output[m] = b[0] * input[n] - a[1] * _prev;
                _prev = output[m];
            }
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public override void Reset()
        {
            _prev = 0;
        }
    }
}
