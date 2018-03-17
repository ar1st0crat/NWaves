using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for the generator of sawtooth waves
    /// </summary>
    public class SawtoothBuilder : SignalBuilder
    {
        /// <summary>
        /// Lower amplitude level
        /// </summary>
        private float _low;

        /// <summary>
        /// Upper amplitude level
        /// </summary>
        private float _high;

        /// <summary>
        /// Frequency of the sawtooth wave
        /// </summary>
        private float _frequency;

        public SawtoothBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<float>>
            {
                {"low, lo, lower",  param => _low = param},
                {"high, hi, upper", param => _high = param},
                {"frequency, freq", param => _frequency = param},
            };

            _low = -1.0f;
            _high = 1.0f;
            _frequency = 0.0f;
        }

        /// <summary>
        /// Method generates sawtooth wave according to the formula:
        /// 
        ///     s[n] = LO + (HI - LO) * (i / N)
        /// 
        /// where i = n % N
        ///       N = fs / freq
        /// </summary>
        /// <returns></returns>
        protected override DiscreteSignal Generate()
        {
            if (_frequency <= 0)
            {
                throw new FormatException("Frequency must be positive!");
            }

            if (_high < _low)
            {
                throw new FormatException("Upper level must be greater than the lower one!");
            }

            var n = SamplingRate / _frequency;
            var start = (int)(n / 2);

            var samples = Enumerable.Range(start, Length)
                                    .Select(i => _low + (_high - _low) * (i % n) / n);

            return new DiscreteSignal(SamplingRate, samples);
        }
    }
}
