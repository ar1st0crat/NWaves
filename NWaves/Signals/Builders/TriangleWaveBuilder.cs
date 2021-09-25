using System;
using System.Collections.Generic;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Builder of triangle waves. 
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    ///     <item>"frequency", "freq" (default: 100.0 Hz)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class TriangleWaveBuilder : SignalBuilder
    {
        /// <summary>
        /// Lower amplitude level.
        /// </summary>
        private double _low;

        /// <summary>
        /// Upper amplitude level.
        /// </summary>
        private double _high;

        /// <summary>
        /// Frequency of the triangle wave.
        /// </summary>
        private double _frequency;

        /// <summary>
        /// Construct <see cref="TriangleWaveBuilder"/>.
        /// </summary>
        public TriangleWaveBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, min",    param => _low = param },
                { "high, hi, max",   param => _high = param },
                { "frequency, freq", param => 
                                     { 
                                         _frequency = param;
                                         _cycles = SamplingRate / _frequency;
                                         _n = (int)(_cycles / 4);
                                     }
                }
            };

            _low = -1.0;
            _high = 1.0;
            _frequency = 100.0;
        }

        /// <summary>
        /// Generate new sample.
        /// </summary>
        public override float NextSample()
        {
            // Triangle wave is generated according to the formula:
            // 
            //     s[n] = LO + 2 * (HI - LO) * (i / N)          when i is less than N / 2
            //            HI + 2 * (LO - HI) * ((i - N/2) / N)  when i is less than N
            // 
            // where i = n % N
            //       N = fs / freq
            //       

            var x = _n % _cycles;
            var sample = x < _cycles / 2 ?
                                _low + 2 * x * (_high - _low) / _cycles :
                                _high + 2 * (x - _cycles / 2) * (_low - _high) / _cycles;
            _n++;
            return (float)sample;
        }

        /// <summary>
        /// Reset sample generator.
        /// </summary>
        public override void Reset()
        {
            _n = (int)(_cycles / 4);
        }

        /// <summary>
        /// Set the sampling rate of the signal to build.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        public override SignalBuilder SampledAt(int samplingRate)
        {
            _cycles = samplingRate / _frequency;
            _n = (int)(_cycles / 4);
            return base.SampledAt(samplingRate);
        }

        /// <summary>
        /// Generate signal by generating all its samples one-by-one. 
        /// Frequency must be greater than zero. 
        /// Upper amplitude must be greater than lower amplitude.
        /// </summary>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_frequency, "Frequency");
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");
            return base.Generate();
        }

        private int _n;

        private double _cycles;
    }
}
