using System;
using System.Collections.Generic;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for the generator of periodic pulse waves
    /// </summary>
    public class PulseWaveBuilder : SignalBuilder
    {
        /// <summary>
        /// Amplitude
        /// </summary>
        private double _amplitude;

        /// <summary>
        /// Pulse duration
        /// </summary>
        private double _pulse;

        /// <summary>
        /// Period of pulse wave
        /// </summary>
        private double _period;

        /// <summary>
        /// Constructor
        /// </summary>
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
            Guard.AgainstNonPositive(_period, "Period");
            Guard.AgainstNonPositive(_pulse, "Pulse duration");

            Guard.AgainstInvalidRange(_pulse, _period, "Pulse duration", "Period");

            var ones = new DiscreteSignal(SamplingRate, (int)(_pulse * SamplingRate), (float)_amplitude);
            var zeros = new DiscreteSignal(SamplingRate, (int)((_period - _pulse) * SamplingRate), 0.0f);

            var repeatTimes = Length / (int)(_period * SamplingRate) + 1;
            var repeated = (ones.Concatenate(zeros)) * repeatTimes;
            
            return repeated.First(Length);
        }
    }
}
