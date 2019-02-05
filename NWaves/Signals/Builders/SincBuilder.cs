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
        /// Amplitude of sinc
        /// </summary>
        private double _amplitude;

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
                { "amplitude, amp, gain", param => _amplitude = param },
                { "frequency, freq",      param => _frequency = param }
            };

            _amplitude = 1.0;
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
            var sample = (float)(_amplitude * MathUtils.Sinc(_n * _frequency / SamplingRate));
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
