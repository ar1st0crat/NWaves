using NWaves.Filters.Base;
using NWaves.Signals;
using System;

namespace NWaves.Operations
{
    /// <summary>
    /// Represents envelope follower (envelope detector).
    /// </summary>
    public class EnvelopeFollower : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Gets or sets attack time (in seconds).
        /// </summary>
        public float AttackTime
        {
            get => _attackTime;
            set
            {
                _attackTime = value;
                _ga = value < 1e-20 ? 0 : (float)Math.Exp(-1.0 / (value * _fs));
            }
        }
        private float _attackTime;

        /// <summary>
        /// Gets or sets release time (in seconds).
        /// </summary>
        public float ReleaseTime
        {
            get => _releaseTime;
            set
            {
                _releaseTime = value;
                _gr = value < 1e-20 ? 0 : (float)Math.Exp(-1.0 / (value * _fs));
            }
        }
        private float _releaseTime;

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Current envelope sample.
        /// </summary>
        private float _env;

        /// <summary>
        /// Attack coefficient.
        /// </summary>
        private float _ga;

        /// <summary>
        /// Release coefficient.
        /// </summary>
        private float _gr;

        /// <summary>
        /// Constructs <see cref="EnvelopeFollower"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="attackTime">Attack time (in seconds)</param>
        /// <param name="releaseTime">Release time (in seconds)</param>
        public EnvelopeFollower(int samplingRate, float attackTime = 0.01f, float releaseTime = 0.05f)
        {
            _fs = samplingRate;
            AttackTime = attackTime;
            ReleaseTime = releaseTime;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public float Process(float sample)
        {
            // envelope following is essentially a low-pass filtering

            var s = Math.Abs(sample);

            _env = _env < s ? _ga * _env + (1 - _ga) * s : _gr * _env + (1 - _gr) * s;

            return _env;
        }

        /// <summary>
        /// Resets envelope follower.
        /// </summary>
        public void Reset()
        {
            _env = 0;
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new signal (envelope).
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
