using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Flanger effect
    /// </summary>
    public class FlangerEffect : AudioEffect
    {
        /// <summary>
        /// Fractional delay line
        /// </summary>
        private readonly FractionalDelayLine _delayLine;

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Width (in seconds)
        /// </summary>
        private float _width;
        public float Width
        {
            get => _width;
            set
            {
                _delayLine.Ensure(_fs, value);
                _width = value;
            }
        }

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
        /// Interpolation mode
        /// </summary>
        public InterpolationMode InterpolationMode
        {
            get => _delayLine.InterpolationMode;
            set => _delayLine.InterpolationMode = value;
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
                             bool inverted = false,
                             InterpolationMode interpolationMode = InterpolationMode.Linear,
                             float reserveWidth = 0/*sec*/)
        {
            _fs = samplingRate;
            _width = width;
            Depth = depth;
            Feedback = feedback;
            Inverted = inverted;
            
            Lfo = lfo;

            if (reserveWidth < width)
            {
                _delayLine = new FractionalDelayLine(samplingRate, width, interpolationMode);
            }
            else
            {
                _delayLine = new FractionalDelayLine(samplingRate, reserveWidth, interpolationMode);
            }
        }

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
                             bool inverted = false,
                             InterpolationMode interpolationMode = InterpolationMode.Linear,
                             float reserveWidth = 0/*sec*/)

            : this(samplingRate, new SineBuilder().SampledAt(samplingRate), width, depth, feedback, inverted, interpolationMode, reserveWidth)
        {
            LfoFrequency = lfoFrequency;
        }

        /// <summary>
        /// Simple flanger effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var delay = _lfo.NextSample() * _width * _fs;

            var delayedSample = _delayLine.Read(delay);

            _delayLine.Write(sample + Feedback * delayedSample);

            return Inverted ? Dry * sample - Wet * Depth * delayedSample
                            : Dry * sample + Wet * Depth * delayedSample;
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
