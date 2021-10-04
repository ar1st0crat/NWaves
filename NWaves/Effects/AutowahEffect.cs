using NWaves.Effects.Base;
using NWaves.Operations;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Represents AutoWah audio effect (envelope follower + Wah-Wah effect).
    /// </summary>
    public class AutowahEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets minimal LFO frequency (in Hz).
        /// </summary>
        public float MinFrequency { get; set; }

        /// <summary>
        /// Gets or sets maximal LFO frequency (in Hz).
        /// </summary>
        public float MaxFrequency { get; set; }

        /// <summary>
        /// Gets or sets attack time (in seconds).
        /// </summary>
        public float AttackTime
        {
            get => _envelopeFollower.AttackTime;
            set => _envelopeFollower.AttackTime = value;
        }

        /// <summary>
        /// Gets or sets release time (in seconds).
        /// </summary>
        public float ReleaseTime
        {
            get => _envelopeFollower.ReleaseTime;
            set => _envelopeFollower.ReleaseTime = value;
        }

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Internal envelope follower.
        /// </summary>
        private readonly EnvelopeFollower _envelopeFollower;

        /// <summary>
        /// Constructs <see cref="AutowahEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
        /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        /// <param name="attackTime">Attack time (in seconds)</param>
        /// <param name="releaseTime">Release time (in seconds)</param>
        public AutowahEffect(int samplingRate,
                             float minFrequency = 30,
                             float maxFrequency = 2000,
                             float q = 0.5f,
                             float attackTime = 0.01f,
                             float releaseTime = 0.05f)
        {
            _fs = samplingRate;

            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;

            _envelopeFollower = new EnvelopeFollower(samplingRate, attackTime, releaseTime);
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var env = _envelopeFollower.Process(sample) * Math.Sqrt(Q);

            var frequencyRange = Math.PI * (MaxFrequency - MinFrequency) / _fs;
            var minFreq = Math.PI * MinFrequency / _fs;

            var centerFrequency = env * frequencyRange + minFreq;

            var f = (float)(2 * Math.Sin(centerFrequency));

            _yh = sample - _yl - Q * _yb;
            _yb += f * _yh;
            _yl += f * _yb;

            return Wet * _yb + Dry * sample;
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            _yh = _yl = _yb = 0;
            _envelopeFollower.Reset();
        }

        private float _yh, _yb, _yl;
    }
}
