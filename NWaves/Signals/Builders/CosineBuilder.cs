using NWaves.Utils;
using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a simple generator of one sinusoid
    /// </summary>
    public class CosineBuilder : SignalBuilder
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
        /// Frequency of the sinusoid (as a fraction of sampling frequency)
        /// </summary>
        private double _frequency;

        /// <summary>
        /// Initial phase of the sinusoid (in radians)
        /// </summary>
        private double _phase;

        /// <summary>
        /// Constructor
        /// </summary>
        public CosineBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, min",    param => _low = param },
                { "high, hi, max",   param => _high = param },
                { "frequency, freq", param => _frequency = param },
                { "phase, phi",      param => _phase = param }
            };

            _low = -1.0;
            _high = 1.0;
            _frequency = 0.0;
            _phase = 0.0;
        }

        /// <summary>
        /// Method for generating one cosine according to simple formula:
        /// 
        ///     y[n] = A * cos(2 * pi * f / fs * n + phase)
        /// 
        /// </summary>
        /// <returns></returns>
        public override float NextSample()
        {
            var sample = Math.Cos(2 * Math.PI * _frequency / SamplingRate * _n + _phase);

            // map it to [min, max] range:

            sample = _low + (_high - _low) * (1 + sample) / 2;

            _n++;
            return (float)sample;
        }

        public override void Reset()
        {
            _n = 0;
        }

        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_frequency, "Frequency");
            return base.Generate();
        }

        private int _n;
    }
}
