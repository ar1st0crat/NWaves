using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public class SinusoidBuilder : SignalBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        private double _amplitude;

        /// <summary>
        /// 
        /// </summary>
        private double _frequency;

        /// <summary>
        /// 
        /// </summary>
        private double _phase;

        public SinusoidBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "amplitude, amp, gain", param => _amplitude = param },
                { "frequency, freq",      param => _frequency = param },
                { "phase, phi",           param => _phase = param }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteSignal Build()
        {
            // TODO: add checks

            var samples = Enumerable.Range(0, Length)
                                    .Select(i => _amplitude * Math.Sin(2 * Math.PI * _frequency / SamplingRate * i + _phase));

            return new DiscreteSignal(samples, SamplingRate);
        }
    }
}
