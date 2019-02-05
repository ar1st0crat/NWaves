using System;
using System.Collections.Generic;
using NWaves.Utils;

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
        private double _low;

        /// <summary>
        /// Upper amplitude level
        /// </summary>
        private double _high;

        /// <summary>
        /// Frequency of the sawtooth wave
        /// </summary>
        private double _frequency;

        /// <summary>
        /// Constructor
        /// </summary>
        public SawtoothBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                {"low, lo, lower",  param => _low = param},
                {"high, hi, upper", param => _high = param},
                {"frequency, freq", param => { _frequency = param; _cycles = SamplingRate / _frequency; _n = (int)(_cycles / 2); }},
            };

            _low = -1.0;
            _high = 1.0;
            _frequency = 0.0;
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
        public override float NextSample()
        {
            var sample = _low + (_high - _low) * (_n % _cycles) / _cycles;
            _n++;
            return (float)sample;
        }

        public override void Reset()
        {
            _n = (int)(_cycles / 2);
        }

        public override SignalBuilder SampledAt(int samplingRate)
        {
            _cycles = samplingRate / _frequency;
            _n = (int)(_cycles / 2);
            return base.SampledAt(samplingRate);
        }

        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_frequency, "Frequency");
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");
            return base.Generate();
        }

        int _n;
        double _cycles;
    }
}
