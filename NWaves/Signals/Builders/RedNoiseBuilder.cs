using System;
using System.Collections.Generic;

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
        private float _low;

        /// <summary>
        /// Upper amplitude level
        /// </summary>
        private float _high;

        /// <summary>
        /// Constructor
        /// </summary>
        public RedNoiseBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<float>>
            {
                { "low, lo, min",  param => _low = param },
                { "high, hi, max", param => _high = param }
            };

            _low = -1.0f;
            _high = 1.0f;
        }

        /// <summary>
        /// Method implements fancy filtering for obtaining the red noise.
        /// </summary>
        /// <returns></returns>
        protected override DiscreteSignal Generate()
        {
            if (_high < _low)
            {
                throw new FormatException("Upper level must be greater than the lower one!");
            }

            var mean = (_low + _high) / 2;
            _low -= mean;
            _high -= mean;

            var rand = new Random();

            float prev = 0;

            var red = new float[Length];
            for (var n = 0; n < Length; n++)
            {
                var white = (float)(rand.NextDouble() * (_high - _low) + _low);

                red[n] = (prev + (0.02f * white)) / 1.02f;
                prev = red[n];
                red[n] *= 3.5f;
                red[n] += mean;
            }

            return new DiscreteSignal(SamplingRate, red);
        }
    }
}
