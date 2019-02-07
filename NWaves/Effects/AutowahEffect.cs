using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
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
        public float Q { get; }

        /// <summary>
        /// Min LFO frequency
        /// </summary>
        public float MinFrequency { get; }

        /// <summary>
        /// Max LFO frequency
        /// </summary>
        public float MaxFrequency { get; }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private int _fs;

        /// <summary>
        /// Envelope follower
        /// </summary>
        private IOnlineFilter _envelopeFollower;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="q"></param>
        public AutowahEffect(int samplingRate, float minFrequency = 30, float maxFrequency = 2000, float q = 0.5f)
        {
            _fs = samplingRate;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;

            Wet = 0.65f;
            Dry = 1 - Wet;

            _envelopeFollower = new LowPassFilter(0.05);
        }

        public override float Process(float sample)
        {
            var filt = _envelopeFollower.Process(Math.Abs(sample)) * 0.4;

            var frequencyRange = Math.PI * (MaxFrequency - MinFrequency) / _fs;
            var minFreq = Math.PI * MinFrequency / _fs;
            var maxFreq = Math.PI * MaxFrequency / _fs;

            var centerFrequency = filt * frequencyRange + minFreq;

            if (centerFrequency > maxFreq)
            {
                centerFrequency = 2*maxFreq;
            }
            if (centerFrequency < minFreq)
            {
                centerFrequency = minFreq;
            }

            _f = (float)(2 * Math.Sin(centerFrequency));

            _yh = sample - _yl - Q * _yb;
            _yb += _f * _yh;
            _yl += _f * _yb;

            return Wet * _yb + Dry * sample;
        }

        public override void Reset()
        {
            _yh = _yl = _yb = 0;
        }

        private float _yh, _yb, _yl;
        private float _f;
    }
}
