using NWaves.Utils;
using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a simple generator of a sinc-signal
    /// </summary>
    public class SincBuilder : SignalBuilder
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
        /// Frequency of the sinc (as a fraction of sampling frequency)
        /// </summary>
        private double _frequency;

        /// <summary>
        /// Constructor
        /// </summary>
        public SincBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, min",    param => _low = param },
                { "high, hi, max",   param => _high = param },
                { "frequency, freq", param => _frequency = param }
            };

            _low = -1.0;
            _high = 1.0;
            _frequency = 0.0;
        }

        /// <summary>
        /// Method for generating sinc signal according to simple formula:
        /// 
        ///     y[n] = A * sinc(f/fs * n)
        /// 
        /// </summary>
        /// <returns></returns>
        public override float NextSample()
        {
            var sample = (float)(_low + (_high - _low) * MathUtils.Sinc(_n * _frequency / SamplingRate));
            _n++;
            return sample;
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
