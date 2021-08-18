using NWaves.Filters.Base;
using NWaves.Signals;
using System;

namespace NWaves.Operations
{
    /// <summary>
    /// Envelope follower (detector)
    /// </summary>
    public class EnvelopeFollower : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Attack time
        /// </summary>
        private float _attackTime;
        public float AttackTime
        {
            get => _attackTime;
            set
            {
                _attackTime = value;
                _ga = value < 1e-20 ? 0 : (float)Math.Exp(-1.0 / (value * _fs));
            }
        }

        /// <summary>
        /// Release time
        /// </summary>
        private float _releaseTime;
        public float ReleaseTime
        {
            get => _releaseTime;
            set
            {
                _releaseTime = value;
                _gr = value < 1e-20 ? 0 : (float)Math.Exp(-1.0 / (value * _fs));
            }
        }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Current envelope sample
        /// </summary>
        private float _env;

        /// <summary>
        /// Attack coefficient
        /// </summary>
        private float _ga;

        /// <summary>
        /// Release coefficient
        /// </summary>
        private float _gr;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="attackTime"></param>
        /// <param name="releaseTime"></param>
        public EnvelopeFollower(int samplingRate, float attackTime = 0.01f, float releaseTime = 0.05f)
        {
            _fs = samplingRate;
            AttackTime = attackTime;
            ReleaseTime = releaseTime;
        }

        /// <summary>
        /// Envelope following is essentialy a low-pass filtering
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float Process(float input)
        {
            var sample = Math.Abs(input);

            _env = _env < sample ? _ga * _env + (1 - _ga) * sample : _gr * _env + (1 - _gr) * sample;

            return _env;
        }

        public void Reset()
        {
            _env = 0;
        }

        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
