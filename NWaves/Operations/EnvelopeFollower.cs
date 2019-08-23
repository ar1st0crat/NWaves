using NWaves.Filters.Base;
using System;

namespace NWaves.Operations
{
    /// <summary>
    /// Envelope follower (detector)
    /// </summary>
    public class EnvelopeFollower : IOnlineFilter
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
                _ga = (float)Math.Exp(-1.0 / (value * _fs));
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
                _gr = (float)Math.Exp(-1.0 / (value * _fs));
            }
        }

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
        /// Sampling rate
        /// </summary>
        private int _fs;

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

            _env = _env < sample ? _ga * _env + (1 - _ga) * sample : _gr * _env + (1 - _ga) * sample;

            return _env;
        }

        public void Reset()
        {
            _env = 0;
        }
    }
}
