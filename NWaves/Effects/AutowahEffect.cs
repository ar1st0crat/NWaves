using NWaves.Operations;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for AutoWah effect.
    /// </summary>
    public class AutowahEffect : AudioEffect
    {
        /// <summary>
        /// Q
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Min LFO frequency
        /// </summary>
        public float MinFrequency { get; set; }

        /// <summary>
        /// Max LFO frequency
        /// </summary>
        public float MaxFrequency { get; set; }

        /// <summary>
        /// Attack time
        /// </summary>
        public float AttackTime
        {
            get => _envelopeFollower.AttackTime;
            set => _envelopeFollower.AttackTime = value;
        }

        /// <summary>
        /// Release time
        /// </summary>
        public float ReleaseTime
        {
            get => _envelopeFollower.ReleaseTime;
            set => _envelopeFollower.ReleaseTime = value;
        }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Envelope follower
        /// </summary>
        private readonly EnvelopeFollower _envelopeFollower;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="q"></param>
        /// <param name="attackTime"></param>
        /// <param name="releaseTime"></param>
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
        /// Autowah means: 1) envelope follower + 2) wahwah effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var filt = _envelopeFollower.Process(sample) * Math.Sqrt(Q);

            var frequencyRange = Math.PI * (MaxFrequency - MinFrequency) / _fs;
            var minFreq = Math.PI * MinFrequency / _fs;

            var centerFrequency = filt * frequencyRange + minFreq;

            var f = (float)(2 * Math.Sin(centerFrequency));

            _yh = sample - _yl - Q * _yb;
            _yb += f * _yh;
            _yl += f * _yb;

            return Wet * _yb + Dry * sample;
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public override void Reset()
        {
            _yh = _yl = _yb = 0;
            _envelopeFollower.Reset();
        }

        private float _yh, _yb, _yl;
    }
}
