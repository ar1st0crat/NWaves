using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Average White Gaussian Noise
    /// </summary>
    public class AwgnBuilder : SignalBuilder
    {
        /// <summary>
        /// Mean
        /// </summary>
        private double _mu;

        /// <summary>
        /// Standard deviation
        /// </summary>
        private double _sigma;

        /// <summary>
        /// Constructor
        /// </summary>
        public AwgnBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "mu, mean",  param => _mu = param },
                { "sigma, stddev", param => _sigma = param }
            };

            _mu = 0.0;
            _sigma = 1.0;
        }

        /// <summary>
        /// Method generates additive white gaussian noise by Box-Muller transform.
        /// </summary>
        /// <returns></returns>
        protected override DiscreteSignal Generate()
        {
            var rand = new Random();
            var noise = new float[Length];

            var i = 0;
            while (i < noise.Length)
            {
                var u1 = rand.NextDouble();
                var u2 = rand.NextDouble();

                var r = Math.Sqrt(-2 * Math.Log(u1));
                var theta = 2 * Math.PI * u2;

                noise[i++] = (float)(r * Math.Cos(theta) * _sigma + _mu);
                if (i < noise.Length) noise[i++] = (float)(r * Math.Sin(theta) * _sigma + _mu);
            }

            return new DiscreteSignal(SamplingRate, noise);
        }
    }
}
