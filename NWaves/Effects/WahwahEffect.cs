using System;
using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;

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
        private float _lfoFrequency;
        public float LfoFrequency
        {
            get => _lfoFrequency;
            set
            {
                _lfoFrequency = value;
                Lfo.SetParameter("freq", value);
            }
        }

        /// <summary>
        /// Min LFO frequency
        /// </summary>
        private float _minFrequency;
        public float MinFrequency
        {
            get => _minFrequency;
            set
            {
                _minFrequency = value;
                Lfo.SetParameter("min", value);
            }
        }

        /// <summary>
        /// Max LFO frequency
        /// </summary>
        private float _maxFrequency;
        public float MaxFrequency
        {
            get => _maxFrequency;
            set
            {
                _maxFrequency = value;
                Lfo.SetParameter("max", value);
            }
        }

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
        private readonly int _fs;


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

            var f = (float)(2 * Math.Sin(Lfo.NextSample() * fs2pi));

            _yh = sample - _yl - Q * _yb;
            _yb += f * _yh;
            _yl += f * _yb;

            return _yb * Wet + sample * Dry;
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public override void Reset()
        {
            _yh = _yb = _yl = 0;
            Lfo.Reset();
        }

        private float _yh, _yb, _yl;
    }
}
