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
        /// LFO frequency
        /// </summary>
        public float LfoFrequency { set { Lfo.SetParameter("freq", value); } }

        /// <summary>
        /// Min LFO frequency
        /// </summary>
        public float MinFrequency { set { Lfo.SetParameter("min", value); } }

        /// <summary>
        /// Max LFO frequency
        /// </summary>
        public float MaxFrequency { set { Lfo.SetParameter("max", value); } }

        /// <summary>
        /// Q
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// LFO object
        /// </summary>
        public SignalBuilder Lfo { get; set; }

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
        public WahwahEffect(int samplingRate,
                            float lfoFrequency = 1.0f,
                            float minFrequency = 300,
                            float maxFrequency = 1500,
                            float q = 0.5f)
        {
            _fs = samplingRate;

            Lfo = new TriangleWaveBuilder().SampledAt(samplingRate);

            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfo"></param>
        /// <param name="q"></param>
        public WahwahEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)
        {
            _fs = samplingRate;
            Q = q;
            Lfo = lfo;
        }

        /// <summary>
        /// Method implements simple wah-wah effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var fs2pi = 2 * Math.PI / _fs;

            _f = (float)(2 * Math.Sin(Lfo.NextSample() * fs2pi));

            _yh = sample - _yl - Q * _yb;
            _yb += _f * _yh;
            _yl += _f * _yb;

            return _yb * Wet + sample * Dry;
        }

        public override void Reset()
        {
            _yh = _yb = _yl = 0;
            Lfo.Reset();
        }

        private float _yh, _yb, _yl;
        private float _f;
    }
}
