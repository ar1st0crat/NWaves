using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System.Linq;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Base abstract class for adaptive filters
    /// </summary>
    public abstract class AdaptiveFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Order
        /// </summary>
        protected readonly int _order;

        /// <summary>
        /// Weights
        /// </summary>
        protected float[] _w;
        public float[] Weights => _w;

        /// <summary>
        /// Delay line
        /// </summary>
        protected float[] _x;

        /// <summary>
        /// Current offset in delay line
        /// </summary>
        protected int _delayLineOffset;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="weights"></param>
        public AdaptiveFilter(int order, float[] weights = null)
        {
            _order = order;

            _w = weights ?? new float[_order];
            Guard.AgainstInequality(order, _w.Length, "Filter order", "Weights array size");

            _x = new float[_order];
        }

        /// <summary>
        /// Process input and desired samples
        /// </summary>
        /// <param name="input"></param>
        /// <param name="desired"></param>
        /// <returns></returns>
        public abstract float Process(float input, float desired);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float Process(float input)
        {
            var y = 0f;

            _x[_delayLineOffset] = input;

            var pos = 0;
            for (var k = _delayLineOffset; k < _order; k++)
            {
                y += _w[pos++] * _x[k];
            }
            for (var k = 0; k < _delayLineOffset; k++)
            {
                y += _w[pos++] * _x[k];
            }
            if (--_delayLineOffset < 0)
            {
                _delayLineOffset = _x.Length - 1;
            }

            return y;
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public void Reset()
        {
            for (var i = 0; i < _x.Length; i++)
            {
                _x[i] = 0;
            }
            _delayLineOffset = _x.Length - 1;
        }

        /// <summary>
        /// Offline filtering
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
