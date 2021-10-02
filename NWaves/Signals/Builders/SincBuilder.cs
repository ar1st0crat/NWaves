using NWaves.Signals.Builders.Base;
using NWaves.Utils;
using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Represents builder of signal Sinc(x).
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    ///     <item>"frequency", "freq" (default: 100.0 Hz)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class SincBuilder : SignalBuilder
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
        /// Frequency of the sinc (Hz).
        /// </summary>
        private double _frequency;

        /// <summary>
        /// Constructs <see cref="SincBuilder"/>.
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
            _frequency = 100.0;
        }

        /// <summary>
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            // y[n] = A * sinc(f / fs * n)

            var sample = (float)(_low + (_high - _low) * MathUtils.Sinc(_n * _frequency / SamplingRate));
            _n++;
            return sample;
        }

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public override void Reset()
        {
            _n = 0;
        }

        /// <summary>
        /// Generates signal by generating all its samples one-by-one. 
        /// Frequency must be greater than zero.
        /// </summary>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstNonPositive(_frequency, "Frequency");
            return base.Generate();
        }

        private int _n;
    }
}
