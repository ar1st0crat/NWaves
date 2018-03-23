using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Utils;

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

        /// <summary>
        /// Constructor
        /// </summary>
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
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_frequency, "Frequency");
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");

            var n = SamplingRate / _frequency;
            
            var samples = Enumerable.Range(0, Length)
                                    .Select(i =>
                                    {
                                        var x = i % n;
                                        return x < n / 2 ? _high : _low;
                                    });

            return new DiscreteSignal(SamplingRate, samples.ToFloats());
        }
    }
}
