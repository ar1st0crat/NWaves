using NWaves.Signals.Builders.Base;
using System;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for generating signals using Karplus-Strong algorithm.
    /// <para>
    /// Parameters that can be set in method <see cref="SignalBuilder.SetParameter(string, double)"/>: 
    /// <list type="bullet">
    ///     <item>"frequency", "freq", "f" (default: 100.0 Hz)</item>
    ///     <item>"stretch", "s" (default: 1.0)</item>
    ///     <item>"feedback", "a" (default: 1.0)</item>
    /// </list>
    /// </para>
    /// </summary>
    public class KarplusStrongBuilder : WaveTableBuilder
    {
        /// <summary>
        /// Frequency (in Hz).
        /// </summary>
        protected double _frequency = 100.0;/*Hz*/
        
        /// <summary>
        /// Stretch factor (0, +INF].
        /// </summary>
        protected double _stretchFactor = 1;

        /// <summary>
        /// Feedback coefficient [0, 1].
        /// </summary>
        protected float _feedback = 1;

        /// <summary>
        /// Constructs <see cref="KarplusStrongBuilder"/>.
        /// </summary>
        public KarplusStrongBuilder() : base(null)
        {
            Init();
        }

        /// <summary>
        /// Constructs <see cref="KarplusStrongBuilder"/> from array of <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">Array of samples</param>
        public KarplusStrongBuilder(float[] samples) : base(samples)
        {
            Init();
        }

        private void Init()
        {
            ParameterSetters.Add("freq, f, frequency", param => SetFrequency(param));
            ParameterSetters.Add("stretch, s", param => _stretchFactor = param);
            ParameterSetters.Add("feedback, a", param => _feedback = (float)param);
        }

        private void SetFrequency(double param)
        {
            _frequency = param;

            if (SamplingRate > 0)
            {
                GenerateWaveTable(SamplingRate / (int)_frequency);
            }
        }

        /// <summary>
        /// Generate wave table of length <paramref name="sampleCount"/>.
        /// </summary>
        /// <param name="sampleCount">Number of wave table samples</param>
        protected void GenerateWaveTable(int sampleCount)
        {
            var values = new[] { -1f, 1f };

            _samples = Enumerable.Range(0, sampleCount)
                                 .Select(_ => values[_rand.Next(2)])
                                 .ToArray();
        }

        /// <summary>
        /// Generate new sample.
        /// </summary>
        public override float NextSample()
        {
            var idx = ((int)_n) % _samples.Length;

            if (_rand.NextDouble() < 1 / _stretchFactor)
            {
                _samples[idx] = 0.5f * (_samples[idx] + _prev) * _feedback;
            }

            _prev = _samples[idx];
            _n++;

            return _prev;
        }

        /// <summary>
        /// Reset sample generator.
        /// </summary>
        public override void Reset()
        {
            var values = new[] { -1f, 1f };

            for (var i = 0; i < _samples.Length; i++)
            {
                _samples[i] = values[_rand.Next(2)];
            }

            base.Reset();
        }

        /// <summary>
        /// Set the sampling rate of the signal to build.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        public override SignalBuilder SampledAt(int samplingRate)
        {
            if (_frequency > 0)
            {
                GenerateWaveTable(samplingRate / (int)_frequency);
            }

            return base.SampledAt(samplingRate);
        }

        /// <summary>
        /// Previous sample.
        /// </summary>
        protected float _prev;

        /// <summary>
        /// Randomizer.
        /// </summary>
        protected readonly Random _rand = new Random();
    }
}
