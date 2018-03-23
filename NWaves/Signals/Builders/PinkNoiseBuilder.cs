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
        protected override DiscreteSignal Generate()
        {
            Guard.AgainstInvalidRange(_low, _high, "Upper amplitude", "Lower amplitude");

            var mean = (_low + _high) / 2;
            _low -= mean;
            _high -= mean;

            var rand = new Random();

            double b0 = 0, b1 = 0, b2 = 0, b3 = 0, b4 = 0, b5 = 0, b6 = 0;

            var pink = new float[Length];
            for (var n = 0; n < Length; n++)
            {
                var white = rand.NextDouble() * (_high - _low) + _low;

                b0 = 0.99886f * b0 + white * 0.0555179f;
                b1 = 0.99332f * b1 + white * 0.0750759f;
                b2 = 0.96900f * b2 + white * 0.1538520f;
                b3 = 0.86650f * b3 + white * 0.3104856f;
                b4 = 0.55000f * b4 + white * 0.5329522f;
                b5 = -0.7616f * b5 - white * 0.0168980f;
                pink[n] = (float)((b0 + b1 + b2 + b3 + b4 + b5 + b6 + white * 0.5362) * 0.11 + mean);
                b6 = white * 0.115926;
            }

            return new DiscreteSignal(SamplingRate, pink);
        }
    }
}
