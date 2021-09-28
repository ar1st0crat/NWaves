using NWaves.Filters.Base;

namespace NWaves.Filters.OnePole
{
    /// <summary>
    /// Represents one-pole IIR filter.
    /// </summary>
    public class OnePoleFilter : IirFilter
    {
        /// <summary>
        /// Delay line.
        /// </summary>
        private float _prev;

        /// <summary>
        /// Constructs <see cref="OnePoleFilter"/>.
        /// </summary>
        protected OnePoleFilter() : base(new[] { 1.0 }, new[] { 1.0, 0 })
        {
        }

        /// <summary>
        /// Constructs <see cref="OnePoleFilter"/> from filter coefficients.
        /// </summary>
        /// <param name="b">Numerator coefficient</param>
        /// <param name="a">Pole</param>
        public OnePoleFilter(double b, double a) : base(new[] { b }, new [] { 1.0, a })
        {
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = _b[0] * sample - _a[1] * _prev;
            _prev = output;

            return output;
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public override void Reset()
        {
            _prev = 0;
        }
    }
}
