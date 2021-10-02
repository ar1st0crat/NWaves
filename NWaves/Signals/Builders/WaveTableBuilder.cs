using NWaves.Signals.Builders.Base;
using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Represents builder of signals that uses a wave table.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"stride", "step", "delta" (default: 1)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class WaveTableBuilder : SignalBuilder
    {
        /// <summary>
        /// Wave table samples.
        /// </summary>
        protected float[] _samples;

        /// <summary>
        /// Stride.
        /// </summary>
        protected float _stride = 1;

        /// <summary>
        /// Interpolate sample or take the nearest one in the wave table. 
        /// True if the stride is not integer.
        /// </summary>
        protected bool _interpolate;

        /// <summary>
        /// Constructs <see cref="WaveTableBuilder"/> from <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">Wave table samples</param>
        public WaveTableBuilder(float[] samples)
        {
            _samples = samples;

            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "stride, step, delta", param => SetStride(param) }
            };
        }

        private void SetStride(double stride)
        {
            _stride = (float)stride;

            // if the stride is not integer then we'll be interpolating:
            _interpolate = Math.Abs(Math.Round(stride) - stride) > 1e-5;
        }

        /// <summary>
        /// Generates new sample (take or interpolate sample from the wave table).
        /// </summary>
        public override float NextSample()
        {
            var idx = ((int)_n) % _samples.Length;

            if (_interpolate)
            {
                var frac = _n - (int)_n;

                _n += _stride;

                return _samples[idx] + frac * (_samples[(idx + 1) % _samples.Length] - _samples[idx]);
            }
            else
            {
                _n += _stride;

                return _samples[idx];
            }
        }

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public override void Reset()
        {
            _n = 0;
        }

        /// <summary>
        /// Current offset.
        /// </summary>
        protected float _n;
    }
}
