using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for the generator of triangle waves
    /// </summary>
    public class TriangleWaveBuilder : SignalBuilder
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
        public TriangleWaveBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, lower",  param => _low = param },
                { "high, hi, upper", param => _high = param },
                { "frequency, freq", param => _frequency = param }
            };

            _low = -1.0;
            _high = 1.0;
            _frequency = 0.0;
        }

        /// <summary>
        /// Method generates triangle wave according to the formula:
        /// 
        ///     s[n] = LO + 2 * (HI - LO) * (i / N)          when i is less than N / 2
        ///            HI + 2 * (LO - HI) * ((i - N/2) / N)  when i is less than N
        /// 
        /// where i = n % N
        ///       N = fs / freq
        /// </summary>
        /// <returns></returns>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_frequency, "Frequency");
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");

            var n = SamplingRate / _frequency;
            var start = (int)(n / 4);

            var samples = Enumerable.Range(start, Length)
                                    .Select(i =>
                                    {
                                        var x = i % n;
                                        return x < n / 2
                                            ? _low + 2 * x * (_high - _low) / n
                                            : _high + 2 * (x - n / 2) * (_low - _high) / n;
                                    });

            return new DiscreteSignal(SamplingRate, samples.ToFloats());
        }
    }
}
