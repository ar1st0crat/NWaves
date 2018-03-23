using System;
using System.Collections.Generic;
using NWaves.Operations;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for generating chirp signals
    /// </summary>
    public class ChirpBuilder : SignalBuilder
    {
        /// <summary>
        /// Amplitude of the sinusoid
        /// </summary>
        private double _amplitude;

        /// <summary>
        /// Start frequency
        /// </summary>
        private double _f0;

        /// <summary>
        /// End frequency
        /// </summary>
        private double _f1;

        /// <summary>
        /// Constructor
        /// </summary>
        public ChirpBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "amplitude, amp, gain", param => _amplitude = param },
                { "f0, freq0, start",     param => _f0 = param },
                { "f1, freq1, end",       param => _f1 = param },
            };

            _amplitude = 1.0;
            _f0 = 100.0;
            _f1 = 1000.0;
        }

        /// <summary>
        /// Method for generating chirp signal according to formula:
        /// 
        ///     y[n] = A * cos(2 * pi * (f0 + k * n) / fs * n)
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_f0, "Start frequency");
            Guard.AgainstNonPositive(_f1, "End frequency");

            var k = (float)((_f1 - _f0) / Length);
            var f = (float) _f0;
            var amp = (float) _amplitude;

            return Modulate.FrequencyLinear(f, amp, k, Length, SamplingRate);
        }
    }
}
