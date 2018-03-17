using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a simple generator of one sinusoid
    /// </summary>
    public class SinusoidBuilder : SignalBuilder
    {
        /// <summary>
        /// Amplitude of the sinusoid
        /// </summary>
        private float _amplitude;

        /// <summary>
        /// Frequency of the sinusoid (as a fraction of sampling frequency)
        /// </summary>
        private float _frequency;

        /// <summary>
        /// Initial phase of the sinusoid (in radians)
        /// </summary>
        private float _phase;

        public SinusoidBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<float>>
            {
                { "amplitude, amp, gain", param => _amplitude = param },
                { "frequency, freq",      param => _frequency = param },
                { "phase, phi",           param => _phase = param }
            };

            _amplitude = 1.0f;
            _frequency = 0.0f;
            _phase = 0.0f;
        }

        /// <summary>
        /// Method for generating one sinusoid according to simple formula:
        /// 
        ///     y[n] = A * sin(2 * pi * f / fs * n + phase)
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DiscreteSignal Generate()
        {
            if (_frequency <= 0)
            {
                throw new FormatException("Frequency must be positive!");
            }

            var samples = Enumerable.Range(0, Length)
                                    .Select(i => (float)(_amplitude * Math.Sin(2 * Math.PI * _frequency / SamplingRate * i + _phase)));

            return new DiscreteSignal(SamplingRate, samples);
        }
    }
}
