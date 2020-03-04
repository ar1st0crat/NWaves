using System;
using System.Collections.Generic;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Wave table builder
    /// </summary>
    public class WaveTableBuilder : SignalBuilder
    {
        protected float[] _samples;

        protected float _stride = 1;

        protected bool _interpolate;


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

        public override void Reset()
        {
            _n = 0;
        }

        protected float _n;
    }
}
