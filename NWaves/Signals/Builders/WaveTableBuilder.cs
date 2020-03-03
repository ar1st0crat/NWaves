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

        protected double _stride = 1;


        public WaveTableBuilder(float[] samples)
        {
            _samples = samples;

            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "stride, step, delta", param => _stride = param }
            };
        }

        public override float NextSample()
        {
            var idx = ((int)_n) % _samples.Length;

            _n += _stride;

            return _samples[idx];
        }

        public override void Reset()
        {
            _n = 0;
        }

        protected double _n;
    }
}
