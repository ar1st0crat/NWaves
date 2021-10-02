using System;
using System.Collections.Generic;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Represents white noise builder.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class WhiteNoiseBuilder : SignalBuilder
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
        /// Constructs <see cref="WhiteNoiseBuilder"/>.
        /// </summary>
        public WhiteNoiseBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, min",  param => _low = param },
                { "high, hi, max", param => _high = param }
            };

            _low = -1.0;
            _high = 1.0;
        }

        /// <summary>
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            return (float)(_rand.NextDouble() * (_high - _low) + _low);
        }

        /// <summary>
        /// Generates signal by generating all its samples one-by-one. 
        /// Upper amplitude must be greater than lower amplitude.
        /// </summary>
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");
            return base.Generate();
        }

        private readonly Random _rand = new Random();
    }
}
