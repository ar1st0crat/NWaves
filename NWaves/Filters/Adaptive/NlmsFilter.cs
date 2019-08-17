using System.Linq;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Adaptive filter (Normalized Least-Mean-Squares algorithm + Epsilon)
    /// </summary>
    public class NlmsFilter : AdaptiveFilter
    {
        /// <summary>
        /// Mu
        /// </summary>
        private readonly float _mu;

        /// <summary>
        /// Epsilon
        /// </summary>
        private readonly float _eps;

        /// <summary>
        /// Leakage
        /// </summary>
        private readonly float _leakage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="mu"></param>
        /// <param name="eps"></param>
        /// <param name="leakage"></param>
        public NlmsFilter(int order, float mu = 0.75f, float eps = 1, float leakage = 0) : base(order)
        {
            _mu = mu;
            _eps = eps;
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

            var norm = _eps + _delayLine.Sum(x => x * x);

            for (var i = 0; i < _kernelSize; i++, offset++)
            {
                _b[i] = _b[_kernelSize + i] = (1 - _leakage * _mu) * _b[i] + _mu * e * _delayLine[offset] / norm;
            }

            return y;
        }
    }
}
