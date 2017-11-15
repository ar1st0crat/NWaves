using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for the generator of triangle waves
    /// </summary>
    public class TriangleWaveBuilder : SignalBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        private double _low;

        /// <summary>
        /// 
        /// </summary>
        private double _high;

        /// <summary>
        /// 
        /// </summary>
        private double _frequency;

        public TriangleWaveBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, lower",  param => _low = param },
                { "high, hi, upper", param => _high = param },
                { "frequency, freq", param => _frequency = param }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override DiscreteSignal Generate()
        {
            if (_frequency <= 0)
            {
                throw new FormatException("Frequency must be positive!");
            }

            if (_high < _low)
            {
                throw new FormatException("Upper level must be greater than he lower one!");
            }

            var samples = Enumerable.Range(0, Length)
                                    .Select(i => 1 - 4 * (0.5 + 0.5 * i * _frequency + 0.25 - Math.Floor(0.5 * i * _frequency + 0.25)));

            return new DiscreteSignal(SamplingRate, samples);
        }
    }
}
