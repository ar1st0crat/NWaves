using NWaves.Utils;
using System.Linq;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Adaptive filter (Least-Mean-Squares with variable steps)
    /// </summary>
    public class VariableStepLmsFilter : AdaptiveFilter
    {
        /// <summary>
        /// Mu
        /// </summary>
        private readonly float[] _mu;

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
        public VariableStepLmsFilter(int order, float[] mu = null, float leakage = 0) : base(order)
        {
            _mu = mu ?? Enumerable.Repeat(0.75f, order).ToArray();
            Guard.AgainstInequality(order, _mu.Length, "Filter order", "Steps array size");

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
                _b[i] = _b[_kernelSize + i] = (1 - _leakage * _mu[i]) * _b[i] + _mu[i] * e * _delayLine[offset];
            }

            return y;
        }
    }
}
