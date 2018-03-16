using System;
using System.Linq;
using NWaves.Signals;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Adaptive filter (Least-Mean-Squares algorithm)
    /// </summary>
    public class LmsFilter
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly int _order;

        /// <summary>
        /// 
        /// </summary>
        private readonly double _mu;

        /// <summary>
        /// Constructor
        /// </summary>
        public LmsFilter(int order, double mu)
        {
            _order = order;
            _mu = mu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="desired"></param>
        /// <returns></returns>
        public DiscreteSignal Adapt(DiscreteSignal signal, DiscreteSignal desired)
        {
            var rand = new Random();
            var kernel = Enumerable.Range(0, _order).Select(k => 1.0 + rand.NextDouble()).ToArray();
            
            var input = signal.Samples;
            var output = new double[input.Length];

            var pos = _order;

            for (var i = 0; i + _order < input.Length; i++)
            {
                var y = 0.0;
                for (var j = 0; j < _order; j++)
                {
                    y += input[i + j] * kernel[_order - j];
                }

                output[pos] = y;

                var error = desired[i] - y;
                
                for (var j = 0; j < _order; j++)
                {
                    kernel[j] += _mu * input[i + j] * error;
                }
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
