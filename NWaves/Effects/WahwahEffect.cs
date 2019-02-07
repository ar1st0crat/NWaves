using System;
using NWaves.Signals.Builders;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for wah-wah effect
    /// </summary>
    public class WahwahEffect : AudioEffect
    {
        /// <summary>
        /// Q
        /// </summary>
        public float Q { get; }

        /// <summary>
        /// LFO frequency
        /// </summary>
        public float LfoFrequency { get; }

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
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfoFrequency"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="q"></param>
        public WahwahEffect(int samplingRate, float lfoFrequency = 1.0f, float minFrequency = 300, float maxFrequency = 1500, float q = 0.5f)
        {
            _fs = samplingRate;
            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;

            _lfo = new TriangleWaveBuilder()
                            .SetParameter("lo", MinFrequency)
                            .SetParameter("hi", MaxFrequency)
                            .SetParameter("freq", LfoFrequency)
                            .SampledAt(samplingRate);
        }

        /// <summary>
        /// Method implements simple wah-wah effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var fs2pi = 2 * Math.PI / _fs;

            _f = (float)(2 * Math.Sin(_lfo.NextSample() * fs2pi));

            _yh = sample - _yl - Q * _yb;
            _yb += _f * _yh;
            _yl += _f * _yb;

            return _yb * Wet + sample * Dry;
        }

        public override void Reset()
        {
            _yh = _yb = _yl = 0;
        }

        private SignalBuilder _lfo;

        private float _yh, _yb, _yl;
        private float _f;
    }
}
