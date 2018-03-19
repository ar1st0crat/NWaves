using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for the generator of periodic pulse waves
    /// </summary>
    public class PulseWaveBuilder : SignalBuilder
    {
        private double _amplitude;
        private double _pulse;
        private double _period;
        
        public PulseWaveBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "amplitude, amp, a", param => _amplitude = param },
                { "pulse, width", param => _pulse = param },
                { "period, t", param => _period = param }
            };

            _amplitude = 1.0;
            _pulse = 0.0;
            _period = 0.0;
        }

        /// <summary>
        /// Method generates simple sequence of rectangular pulses.
        /// </summary>
        /// <returns></returns>
        protected override DiscreteSignal Generate()
        {
            if (_period <= _pulse)
            {
                throw new FormatException("The period must be greater than pulse duration!");
            }

            if (_period <= 0.0 || _pulse <= 0.0)
            {
                throw new FormatException("The period and pulse duration must be positive!");
            }

            var ones = new DiscreteSignal(SamplingRate, (int)(_pulse * SamplingRate), (float)_amplitude);
            var zeros = new DiscreteSignal(SamplingRate, (int)((_period - _pulse) * SamplingRate), 0.0f);

            var repeatTimes = Length / (int)(_period * SamplingRate) + 1;
            var repeated = (ones + zeros) * repeatTimes;
            
            return repeated.First(Length);
        }
    }
}
