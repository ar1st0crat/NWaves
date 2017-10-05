using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public class SawtoothBuilder : SignalBuilder
    {
        private double _low;
        private double _high;
        private double _frequency;
        private double _phase;

        public SawtoothBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                {"low, lo, lower",  param => _low = param},
                {"high, hi, upper", param => _high = param},
                {"frequency, freq", param => _frequency = param},
                {"phase, phi",      param => _phase = param}
            };
        }

        /// <summary>
        /// Formula:
        /// 
        ///     s[n] = LO + (HI - LO) * frac(i * freq + phi)
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteSignal Build()
        {
            // TODO: add checks

            var samples = Enumerable.Range(0, Length)
                                    .Select(i => _low + (_high - _low) * ((i*_frequency + _phase) - Math.Floor(i*_frequency + _phase)));

            return new DiscreteSignal(SamplingRate, samples);
        }
    }
}
