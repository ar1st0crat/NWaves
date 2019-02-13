using NWaves.Signals.Builders;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Flanger effect.
    /// 
    /// It's almost identical to vibrato effect
    /// except that LFO is always sinusoidal
    /// and the original signal is superimposed (wet/dry).
    /// </summary>
    public class FlangerEffect : AudioEffect
    {
        /// <summary>
        /// Max delay (in seconds)
        /// </summary>
        public float MaxDelay { get; }

        /// <summary>
        /// LFO frequency
        /// </summary>
        public float LfoFrequency { get; }

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
        public FlangerEffect(int samplingRate, float maxDelay = 0.003f/*sec*/, float lfoFrequency = 1/*Hz*/)
        {
            _fs = samplingRate;
            MaxDelay = maxDelay;
            
            _lfo = new SineBuilder()
                            .SetParameter("freq", lfoFrequency)
                            .SampledAt(samplingRate);

            _maxDelayPos = (int)(Math.Ceiling(samplingRate * maxDelay));
            _delayLine = new float[_maxDelayPos + 1];
        }

        /// <summary>
        /// Simple flanger effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var preciseDelay = (_lfo.NextSample() + 1) / 2 * _maxDelayPos;

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
            
            return Dry * sample + Wet * delayedSample;
        }

        public override void Reset()
        {
            _n = 1;

            for (var i = 0; i < _delayLine.Length; i++)
            {
                _delayLine[i] = 0;
            }
        }

        private SignalBuilder _lfo;

        private float[] _delayLine;
        private int _maxDelayPos;

        private int _n = 1;
    }
}
