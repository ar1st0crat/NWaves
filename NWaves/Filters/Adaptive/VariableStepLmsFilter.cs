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
        protected readonly float _leakage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="mu"></param>
        /// <param name="weights"></param>
        /// <param name="leakage"></param>
        public VariableStepLmsFilter(int order,
                                     float[] mu = null,
                                     float[] weights = null,
                                     float leakage = 0)
            : base(order, weights)
        {
            _mu = mu ?? Enumerable.Repeat(0.1f, order).ToArray();
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
            var y = Process(input);

            var e = desired - y;

            for (var i = 0; i < _order; i++)
            {
                _w[i] = (1 - _leakage * _mu[i]) * _w[i] + _mu[i] * e * _x[i];
            }

            return y;
        }
    }
}
