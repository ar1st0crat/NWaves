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
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = _b32[0] * sample - _a32[1] * _prev;
            _prev = output;

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
