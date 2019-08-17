using System;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Adaptive filter (Sign Least-Mean-Squares algorithm)
    /// </summary>
    public class SignLmsFilter : AdaptiveFilter
    {
        /// <summary>
        /// Mu
        /// </summary>
        private readonly float _mu;

        /// <summary>
        /// Leakage
        /// </summary>
        private readonly float _leakage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="mu"></param>
        /// <param name="leakage"></param>
        public SignLmsFilter(int order, float mu = 0.75f, float leakage = 0) : base(order)
        {
            _mu = mu;
            _leakage = leakage;
        }

        /// <summary>
        /// Process input and desired samples
        /// </summary>
        /// <param name="input"></param>
        /// <param name="desired"></param>
        /// <returns></returns>
        public override float Process(float input, float desired)
        {
            var offset = _delayLineOffset;

            _delayLine[offset + _kernelSize] = input;   // duplicate it for better loop performance


            var y = Process(input);

            var e = desired - y;

            for (var i = 0; i < _kernelSize; i++, offset++)
            {
                _b[i] = _b[_kernelSize + i] = (1 - _leakage * _mu) * _b[i] + _mu * Math.Sign(e) * Math.Sign(_delayLine[offset]);
            }

            return y;
        }
    }
}
