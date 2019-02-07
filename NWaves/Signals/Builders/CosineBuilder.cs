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
        /// Amplitude of the sinusoid
        /// </summary>
        private double _amplitude;

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
                { "amplitude, amp, gain", param => _amplitude = param },
                { "frequency, freq",      param => _frequency = param },
                { "phase, phi",           param => _phase = param }
            };

            _amplitude = 1.0;
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
            var sample = (float)(_amplitude * Math.Cos(2 * Math.PI * _frequency / SamplingRate * _n + _phase));
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
