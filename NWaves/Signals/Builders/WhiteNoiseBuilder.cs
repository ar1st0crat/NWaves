using System;
using System.Collections.Generic;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a white noise generator
    /// </summary>
    public class WhiteNoiseBuilder : SignalBuilder
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
        /// Constructor
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

        protected override DiscreteSignal Generate()
        {
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");
            return base.Generate();
        }

        /// <summary>
        /// Method generates white noise by simply generating 
        /// consecutive decorrelated random samples.
        /// </summary>
        /// <returns></returns>
        public override float NextSample()
        {
            return (float)(_rand.NextDouble() * (_high - _low) + _low);
        }

        private Random _rand = new Random();
    }
}
