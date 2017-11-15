using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a pink noise generator
    /// </summary>
    public class PinkNoiseBuilder : SignalBuilder
    {
        /// <summary>
        /// Lower amplitude level
        /// </summary>
        private double _low;

        /// <summary>
        /// Upper amplitude level
        /// </summary>
        private double _high;

        public PinkNoiseBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, min",  param => _low = param },
                { "high, hi, max", param => _high = param }
            };

            _low = -1.0;
            _high = 1.0;
        }

        /// <summary>
        /// Method implements Paul Kellet's algorithm.
        /// </summary>
        /// <returns></returns>
        public override DiscreteSignal Generate()
        {
            if (_high < _low)
            {
                throw new FormatException("Upper level must be greater than he lower one!");
            }

            var rand = new Random();

            double b0 = 0, b1 = 0, b2 = 0, b3 = 0, b4 = 0, b5 = 0, b6 = 0;

            var pink = new double[Length];
            for (var n = 0; n < Length; n++)
            {
                var white = rand.NextDouble() * (_high - _low) + _low;

                b0 = 0.99886 * b0 + white * 0.0555179;
                b1 = 0.99332 * b1 + white * 0.0750759;
                b2 = 0.96900 * b2 + white * 0.1538520;
                b3 = 0.86650 * b3 + white * 0.3104856;
                b4 = 0.55000 * b4 + white * 0.5329522;
                b5 = -0.7616 * b5 - white * 0.0168980;
                pink[n] = b0 + b1 + b2 + b3 + b4 + b5 + b6 + white * 0.5362;
                pink[n] *= 0.11;
                b6 = white * 0.115926;
            }

            return new DiscreteSignal(SamplingRate, pink);
        }
    }
}
