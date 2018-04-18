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
        /// Online filtering buffer-by-buffer
        /// </summary>
        /// <param name="input">Input buffer</param>
        /// <param name="filteringOptions">Ignored by one-pole filters</param>
        /// <returns></returns>
        public override float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var output = new float[input.Length];

            var a = _a32;
            var b = _b32;

            for (var n = 0; n < input.Length; n++)
            {
                output[n] = b[0] * input[n] - a[1] * _prev;
                _prev = output[n];
            }

            return output;
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
