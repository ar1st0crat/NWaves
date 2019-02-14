using NWaves.Signals.Builders;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Vibrato effect
    /// </summary>
    public class VibratoEffect : AudioEffect
    {
        /// <summary>
        /// LFO frequency
        /// </summary>
        public float LfoFrequency { set { Lfo.SetParameter("freq", value); } }

        /// <summary>
        /// Max delay (in seconds)
        /// </summary>
        public float MaxDelay
        {
            set
            {
                _maxDelayPos = (int)(Math.Ceiling(_fs * value));
                _delayLine = new float[_maxDelayPos + 1];
            }
        }

        /// <summary>
        /// LFO
        /// </summary>
        private SignalBuilder _lfo;
        public SignalBuilder Lfo
        {
            get { return _lfo; }
            set
            {
                _lfo = value;
                _lfo.SetParameter("min", 0.0).SetParameter("max", 1.0);
            }
        }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private int _fs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="maxDelay"></param>
        /// <param name="lfoFrequency"></param>
        public VibratoEffect(int samplingRate, float maxDelay = 0.003f/*sec*/, float lfoFrequency = 1/*Hz*/)
        {
            _fs = samplingRate;

            Lfo = new SineBuilder().SampledAt(samplingRate);

            MaxDelay = maxDelay;
            LfoFrequency = lfoFrequency;
        }

        /// <summary>
        /// Simple flanger effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var preciseDelay = Lfo.NextSample() * _maxDelayPos;

            var delay = (int)preciseDelay;
            var fracDelay = preciseDelay - delay;

            if (_n == _delayLine.Length)
            {
                _n = 1;
            }

            _delayLine[_n] = sample;

            // linear interpolation:

            var offset1 = _n > delay ? _n - delay : _n + _maxDelayPos - delay;
            var offset2 = offset1 == 1 ? _maxDelayPos : offset1 - 1;

            var delayedSample = _delayLine[offset2] + (1 - fracDelay) * (_delayLine[offset1] - _delayLine[offset2]);

            _n++;

            return delayedSample;
            
            // instead of:
            // return Dry * sample + Wet * delayedSample;
        }

        public override void Reset()
        {
            _n = 1;

            for (var i = 0; i < _delayLine.Length; i++)
            {
                _delayLine[i] = 0;
            }

            Lfo.Reset();
        }

        private float[] _delayLine;
        private int _maxDelayPos;

        private int _n = 1;
    }
}
