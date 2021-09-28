using NWaves.Utils;
using System.Linq;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Represents LMS Adaptive filter (Least-Mean-Squares) with variable steps.
    /// </summary>
    public class VariableStepLmsFilter : AdaptiveFilter
    {
        private readonly float[] _mu;
        private readonly float _leakage;

        /// <summary>
        /// Constructs <see cref="VariableStepLmsFilter"/> of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="mu">Mu</param>
        /// <param name="leakage">Leakage</param>
        public VariableStepLmsFilter(int order, float[] mu = null, float leakage = 0) : base(order)
        {
            _mu = mu ?? Enumerable.Repeat(0.75f, order).ToArray();
            Guard.AgainstInequality(order, _mu.Length, "Filter order", "Steps array size");

            _leakage = leakage;
        }

        /// <summary>
        /// Processes one sample of input and desired signals and adapts filter coefficients.
        /// </summary>
        /// <param name="input">Sample of input signal</param>
        /// <param name="desired">Sample of desired signal</param>
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
