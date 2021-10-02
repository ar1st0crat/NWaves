using System;
using System.Collections.Generic;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Represents builder of periodic pulse waves.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    ///     <item>"pulse", "width" (default: 0.05 seconds)</item>
    ///     <item>"period", "t" (default: 0.1 seconds)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class PulseWaveBuilder : SignalBuilder
    {
        /// <summary>
        /// Lower amplitude level.
        /// </summary>
        private double _low;

        /// <summary>
        /// Upper amplitude level.
        /// </summary>
        private double _high;

        /// <summary>
        /// Pulse duration (in seconds).
        /// </summary>
        private double _pulse;

        /// <summary>
        /// Period of pulse wave (in seconds).
        /// </summary>
        private double _period;

        /// <summary>
        /// Constructs <see cref="PulseWaveBuilder"/>.
        /// </summary>
        public PulseWaveBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, min",  param => _low = param },
                { "high, hi, max", param => _high = param },
                { "pulse, width",  param => _pulse = param },
                { "period, t",     param => _period = param }
            };

            _low = -1.0;
            _high = 1.0;
            _pulse = 0.05; // 50 ms
            _period = 0.1; // 100 ms
        }

        /// <summary>
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            var sample = _n <= (int)(_pulse * SamplingRate) ? _high : _low;

            if (++_n == (int)(_period * SamplingRate))
            {
                _n = 0;
            }

            return (float)sample;
        }

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public override void Reset()
        {
            _n = 0;
        }

        /// <summary>
        /// Generates signal by generating all its samples one-by-one. 
        /// Period and pulse duration must be greater than zero. 
        /// Period must be greater than pulse duration.
        /// </summary>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_period, "Period");
            Guard.AgainstNonPositive(_pulse, "Pulse duration");
            Guard.AgainstInvalidRange(_pulse, _period, "Pulse duration", "Period");
            return base.Generate();
        }

        private int _n;
    }
}
