using System;
using System.Collections.Generic;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for generating chirp signals
    /// </summary>
    public class ChirpBuilder : SignalBuilder
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
                { "low, lo, min",     param => _low = param },
                { "high, hi, max",    param => _high = param },
                { "f0, freq0, start", param => _f0 = param },
                { "f1, freq1, end",   param => _f1 = param },
            };

            _low = -1.0;
            _high = 1.0;
            _f0 = 100.0;
            _f1 = 1000.0;
        }

        /// <summary>
        /// Method for generating chirp signal according to formula:
        /// 
        ///     y[n] = A * cos(2 * pi * (f0 + k * n) / fs * n)
        /// 
        /// The same could be achieved via:
        /// 
        ///     new Modulator().FrequencyLinear(f, amp, k, Length, SamplingRate);
        /// 
        /// </summary>
        /// <returns></returns>
        public override float NextSample()
        {
            var k = (float)((_f1 - _f0) / Length);
            var fs = SamplingRate;
            
            var sample = Math.Cos(2 * Math.PI * (_f0 / fs + k * _n) * _n / fs);

            // map it to [min, max] range:

            sample = _low + (_high - _low) * (1 + sample) / 2;

            if (++_n == Length)
            {
                _n = 0;
            }

            return (float)sample;
        }

        public override void Reset()
        {
            _n = 0;
        }

        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_f0, "Start frequency");
            Guard.AgainstNonPositive(_f1, "End frequency");
            return base.Generate();
        }

        private int _n;
    }
}
