using System;
using System.Collections.Generic;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;

namespace NWaves.Signals.Builders
{
    // Implementation of 1D Perlin Noise ported from Stefan Gustavson's code:
    //
    //      https://github.com/stegu/perlin-noise/blob/master/src/noise1234.c
    //

    /// <summary>
    /// Represents Perlin noise builder (1D simplex noise).
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"low", "lo", "min" (default: -1.0)</item>
    ///     <item>"high", "hi", "max" (default: 1.0)</item>
    ///     <item>"scale", "octave" (default: 0.02)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class PerlinNoiseBuilder : SignalBuilder
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
        /// Scale.
        /// </summary>
        private double _scale;

        /// <summary>
        /// Table of permutations.
        /// </summary>
        private readonly byte[] _permutation = new byte[512];

        /// <summary>
        /// Constructs <see cref="PerlinNoiseBuilder"/>.
        /// </summary>
        public PerlinNoiseBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "low, lo, min",  param => _low = param },
                { "high, hi, max", param => _high = param },
                { "scale, octave", param => _scale = param }
            };

            _low = -1.0;
            _high = 1.0;
            _scale = 0.02;

            _rand.NextBytes(_permutation);
        }

        /// <summary>
        /// 1D simplex noise.
        /// </summary>
        private double GenerateSample(double x)
        {
            var i1 = (int)x < x ? (int)x : (int)x - 1;
            var i2 = (i1 + 1) & 0xff;
            var f1 = x - i1;
            var f2 = f1 - 1.0;
            
            i1 &= 0xff;

            return 0.188 * Lerp(Fade(f1), Gradient(_permutation[i1], f1),
                                          Gradient(_permutation[i2], f2));
        }

        /// <summary>
        /// Gradient.
        /// </summary>
        private static double Gradient(int hash, double x)
        {
            var h = hash & 15;
            var g = 1.0 + (h & 7);
            return ((h & 8) == 0) ? g * x : -g * x;
        }

        /// <summary>
        /// Improved interpolator.
        /// </summary>
        private static double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        /// <summary>
        /// Linear interpolator.
        /// </summary>
        private static double Lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }

        /// <summary>
        /// Generates new sample.
        /// </summary>
        public override float NextSample()
        {
            var sample = GenerateSample(_n * _scale) * (_high - _low) / 2 + (_high + _low) / 2;
            _n++;
            return (float)sample;
        }

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public override void Reset()
        {
            _n = 0;
            _rand.NextBytes(_permutation);
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

        private int _n;

        private readonly Random _rand = new Random();
    }
}
