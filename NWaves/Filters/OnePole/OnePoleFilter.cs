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
        /// Constructor
        /// </summary>
        protected OnePoleFilter() : base(new[] { 1.0 }, new[] { 1.0, 0 })
        {
        }

        /// <summary>
        /// Constructor for user defined coefficients
        /// </summary>
        /// <param name="b"></param>
        /// <param name="a"></param>
        protected OnePoleFilter(double b, double a) : base(new[] { b }, new [] { 1.0, a })
        {
        }

        /// <summary>
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = _b[0] * sample - _a[1] * _prev;
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
