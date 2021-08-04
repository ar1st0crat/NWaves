using NWaves.Signals.Builders;
using NWaves.Utils;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Vibrato effect
    /// </summary>
    public class VibratoEffect : AudioEffect
    {
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
        /// LFO frequency
        /// </summary>
        private float _lfoFrequency = 1;
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
        public VibratoEffect(int samplingRate, float lfoFrequency = 1/*Hz*/, float width = 0.003f/*sec*/)
        {
            _fs = samplingRate;

            Width = width;

            Lfo = new SineBuilder().SampledAt(samplingRate);
            LfoFrequency = lfoFrequency;
        }

        /// <summary>
        /// Constructor with LFO
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfo"></param>
        /// <param name="width"></param>
        public VibratoEffect(int samplingRate, SignalBuilder lfo, float width = 0.003f/*sec*/)
        {
            _fs = samplingRate;

            Width = width;
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


            _delayLine[_n++] = sample;

            return Dry * sample + Wet * delayedSample;
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

    /// <summary>
    /// 
    /// </summary>
    public class NewVibratoEffect : AudioEffect
    {
        /// <summary>
        /// Fractional delay line
        /// </summary>
        private FractionalDelayLine _delayLine;

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
                _delayLine = new FractionalDelayLine(_fs, _width);
            }
        }

        /// <summary>
        /// LFO frequency
        /// </summary>
        private float _lfoFrequency = 1;
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
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfoFrequency"></param>
        /// <param name="width"></param>
        public NewVibratoEffect(int samplingRate, float lfoFrequency = 1/*Hz*/, float width = 0.003f/*sec*/)
        {
            _fs = samplingRate;

            Width = width;

            Lfo = new SineBuilder().SampledAt(samplingRate);
            LfoFrequency = lfoFrequency;
        }

        /// <summary>
        /// Constructor with LFO
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfo"></param>
        /// <param name="width"></param>
        public NewVibratoEffect(int samplingRate, SignalBuilder lfo, float width = 0.003f/*sec*/)
        {
            _fs = samplingRate;

            Width = width;
            Lfo = lfo;
        }

        /// <summary>
        /// Simple flanger effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var delay = _lfo.NextSample() * _width * _fs;// _maxDelayPos;

            var delayedSample = _delayLine.Read(delay);

            _delayLine.Write(sample);

            return Dry * sample + Wet * delayedSample;
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public override void Reset()
        {
            _delayLine.Reset();
            _lfo.Reset();
        }
    }
}
