using NWaves.Signals.Builders;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Flanger effect
    /// </summary>
    public class FlangerEffect : AudioEffect
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
                _lfo.SetParameter("freq", value);
            }
        }

        /// <summary>
        /// LFO
        /// </summary>
        private SignalBuilder _lfo;
        public SignalBuilder Lfo
        {
            get => _lfo;
            set
            {
                _lfo = value;
                _lfo.SetParameter("min", 0.0).SetParameter("max", 1.0);
            }
        }

        /// <summary>
        /// Width (max delay in seconds)
        /// </summary>
        private float _width;
        public float Width
        {
            get => _width;
            set
            {
                _width = value;
                _maxDelayPos = (int)Math.Ceiling(_fs * value);
                _delayLine = new float[_maxDelayPos + 1];
            }
        }

        /// <summary>
        /// Depth
        /// </summary>
        public float Depth { get; set; }

        /// <summary>
        /// Feedback coefficient
        /// </summary>
        public float Feedback { get; set; }

        /// <summary>
        /// Inverted mode
        /// </summary>
        public bool Inverted { get; set; }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Delay line
        /// </summary>
        private float[] _delayLine;
        private int _maxDelayPos;
        private int _n = 1;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfoFrequency"></param>
        /// <param name="width"></param>
        /// <param name="depth"></param>
        /// <param name="feedback"></param>
        /// <param name="inverted"></param>
        public FlangerEffect(int samplingRate,
                             float lfoFrequency = 1/*Hz*/,
                             float width = 0.003f/*sec*/,
                             float depth = 0.5f,
                             float feedback = 0,
                             bool inverted = false)
        {
            _fs = samplingRate;

            Width = width;
            Depth = depth;
            Feedback = feedback;
            Inverted = inverted;
            
            Lfo = new SineBuilder().SampledAt(samplingRate);
            LfoFrequency = lfoFrequency;
        }

        /// <summary>
        /// Constructor with LFO
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfo"></param>
        /// <param name="width"></param>
        /// <param name="lfoFrequency"></param>
        /// <param name="depth"></param>
        /// <param name="feedback"></param>
        /// <param name="inverted"></param>
        public FlangerEffect(int samplingRate,
                             SignalBuilder lfo,
                             float width = 0.003f/*sec*/,
                             float depth = 0.5f,
                             float feedback = 0,
                             bool inverted = false)
        {
            _fs = samplingRate;

            Width = width;
            Depth = depth;
            Feedback = feedback;
            Inverted = inverted;
            Lfo = lfo;
        }

        /// <summary>
        /// Simple flanger effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            if (_n == _delayLine.Length)
            {
                _n = 1;
            }

            var preciseDelay = _lfo.NextSample() * _maxDelayPos;

            var delay = (int)preciseDelay;
            var fracDelay = preciseDelay - delay;

            // linear interpolation:

            var offset1 = _n > delay ? _n - delay : _n + _maxDelayPos - delay;
            var offset2 = offset1 == 1 ? _maxDelayPos : offset1 - 1;

            var delayedSample = _delayLine[offset2] + (1 - fracDelay) * (_delayLine[offset1] - _delayLine[offset2]);


            _delayLine[_n++] = sample + Feedback * delayedSample;

            return Inverted ? Dry * sample - Wet * Depth * delayedSample
                            : Dry * sample + Wet * Depth * delayedSample;
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public override void Reset()
        {
            Array.Clear(_delayLine, 0, _delayLine.Length);
            _lfo.Reset();
            _n = 1;
        }
    }
}
