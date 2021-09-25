using NWaves.Signals.Builders.Base;
using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// <para>
    /// Additive White Gaussian Noise (AWGN) builder. 
    /// Generates additive white gaussian noise using Box-Muller transform.
    /// </para>
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"mean", "mu" (default: 0.0)</item>
    ///     <item>"sigma", "stddev" (default: 1.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class AwgnBuilder : SignalBuilder
    {
        /// <summary>
        /// Mean.
        /// </summary>
        private double _mu;

        /// <summary>
        /// Standard deviation.
        /// </summary>
        private double _sigma;

        /// <summary>
        /// Constructs <see cref="AwgnBuilder"/>.
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
        /// Generate new sample.
        /// </summary>
        public override float NextSample()
        {
            if (_nextReady)
            {
                return _next;
            }

            var u1 = _rand.NextDouble();
            var u2 = _rand.NextDouble();

            var r = Math.Sqrt(-2 * Math.Log(u1));
            var theta = 2 * Math.PI * u2;

            var sample = (float)(r * Math.Cos(theta) * _sigma + _mu);
            _next = (float)(r * Math.Sin(theta) * _sigma + _mu);

            return sample;
        }

        /// <summary>
        /// Reset sample generator.
        /// </summary>
        public override void Reset()
        {
            _nextReady = false;
        }

        private float _next;
        private bool _nextReady;

        private readonly Random _rand = new Random();
    }
}
