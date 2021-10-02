using System;
using System.Collections.Generic;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Represents pink noise builder. 
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class PinkNoiseBuilder : SignalBuilder
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
        /// Constructs <see cref="PinkNoiseBuilder"/>.
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
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            //  Paul Kellet's algorithm

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

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public override void Reset()
        {
            _b0 = _b1 = _b2 = _b3 = _b4 = _b5 = _b6 = 0;
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

        private double _b0, _b1, _b2, _b3, _b4, _b5, _b6;

        private readonly Random _rand = new Random();
    }
}
