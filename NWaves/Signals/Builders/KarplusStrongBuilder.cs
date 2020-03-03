using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class providing implementation of Karplus-Strong algorithm
    /// </summary>
    public class KarplusStrongBuilder : WaveTableBuilder
    {
        protected double _frequency;

        protected double _stretchFactor = 1;

        public KarplusStrongBuilder() : base(null)
        {
            Init();
        }

        public KarplusStrongBuilder(float[] samples) : base(samples)
        {
            Init();
        }

        private void Init()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "freq, f, frequency", param => SetFrequency(param) },
                { "stretch, s", param => _stretchFactor = param }
            };
        }

        private void SetFrequency(double param)
        {
            _frequency = param;

            if (SamplingRate > 0)
            {
                GenerateWaveTable(SamplingRate / (int)_frequency);
            }
        }

        protected void GenerateWaveTable(int sampleCount)
        {
            var values = new[] { -1f, 1f };

            _samples = Enumerable.Range(0, sampleCount)
                                 .Select(_ => values[_rand.Next(2)])
                                 .ToArray();
        }

        public override float NextSample()
        {
            var idx = ((int)_n) % _samples.Length;

            if (_rand.NextDouble() < 1 / _stretchFactor)
            {
                _samples[idx] = (_samples[idx] + _prev) / 2;
            }

            _prev = _samples[idx];
            _n++;

            return _prev;
        }

        public override void Reset()
        {
            _n = 0;
        }

        public override SignalBuilder SampledAt(int samplingRate)
        {
            if (_frequency > 0)
            {
                GenerateWaveTable(samplingRate / (int)_frequency);
            }

            return base.SampledAt(samplingRate);
        }

        protected float _prev;
        protected Random _rand = new Random();
    }
}
