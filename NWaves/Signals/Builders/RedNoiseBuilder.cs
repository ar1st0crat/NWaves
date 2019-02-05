using System;
using System.Collections.Generic;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a red (Brownian) noise generator
    /// </summary>
    public class RedNoiseBuilder : SignalBuilder
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
        public RedNoiseBuilder()
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
        /// Method implements fancy filtering for obtaining the red noise.
        /// </summary>
        /// <returns></returns>
        public override float NextSample()
        {
            var mean = (_low + _high) / 2;
            _low -= mean;
            _high -= mean;

            var white = _rand.NextDouble() * (_high - _low) + _low;

            var red = (_prev + (0.02 * white)) / 1.02;
            _prev = red;
            return (float)(red * 3.5 + mean);
        }

        public override void Reset()
        {
            _prev = 0;
        }

        protected override DiscreteSignal Generate()
        {
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");
            return base.Generate();
        }

        double _prev;
        Random _rand = new Random();
    }
}
