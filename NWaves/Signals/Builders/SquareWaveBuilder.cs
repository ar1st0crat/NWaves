using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for the generator of triangle waves
    /// </summary>
    public class SquareWaveBuilder : SignalBuilder
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
        /// Frequency of the triangle wave
        /// </summary>
        private double _frequency;

        public SquareWaveBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, lower",  param => _low = param },
                { "high, hi, upper", param => _high = param },
                { "frequency, freq", param => _frequency = param }
            };
        }

        /// <summary>
        /// Method generates square wave
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

            var n = SamplingRate / _frequency;
            
            var samples = Enumerable.Range(0, Length)
                                    .Select(i =>
                                    {
                                        var x = i % n;
                                        return x < n / 2 ? _high : _low;
                                    });

            return new DiscreteSignal(SamplingRate, samples);
        }
    }
}
