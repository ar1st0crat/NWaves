using System;
using System.Collections.Generic;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a pink noise generator
    /// </summary>
    public class PinkNoiseBuilder : SignalBuilder
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
        public PinkNoiseBuilder()
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
        /// Method implements Paul Kellet's algorithm.
        /// </summary>
        /// <returns></returns>
        public override float NextSample()
        {
            var mean = (_low + _high) / 2;
            _low -= mean;
            _high -= mean;

            var white = _rand.NextDouble() * (_high - _low) + _low;

            _b0 = 0.99886f * _b0 + white * 0.0555179f;
            _b1 = 0.99332f * _b1 + white * 0.0750759f;
            _b2 = 0.96900f * _b2 + white * 0.1538520f;
            _b3 = 0.86650f * _b3 + white * 0.3104856f;
            _b4 = 0.55000f * _b4 + white * 0.5329522f;
            _b5 = -0.7616f * _b5 - white * 0.0168980f;
            var pink = (_b0 + _b1 + _b2 + _b3 + _b4 + _b5 + _b6 + white * 0.5362) * 0.11 + mean;
            _b6 = white * 0.115926;

            return (float)pink;
        }

        public override void Reset()
        {
            _b0 = _b1 = _b2 = _b3 = _b4 = _b5 = _b6 = 0;
        }

        protected override DiscreteSignal Generate()
        {
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");
            return base.Generate();
        }

        double _b0, _b1, _b2, _b3, _b4, _b5, _b6;

        Random _rand = new Random();
    }
}
