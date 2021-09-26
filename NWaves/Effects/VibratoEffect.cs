using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Class representing Vibrato audio effect.
    /// </summary>
    public class VibratoEffect : AudioEffect
    {
        /// <summary>
        /// Internal fractional delay line.
        /// </summary>
        private readonly FractionalDelayLine _delayLine;

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Gets or sets width (in seconds).
        /// </summary>
        public float Width
        {
            get => _width;
            set
            {
                _delayLine.Ensure(_fs, value);
                _width = value;
            }
        }
        private float _width;

        /// <summary>
        /// Gets or sets LFO frequency (in Hz).
        /// </summary>
        public float LfoFrequency
        {
            get => _lfoFrequency;
            set
            {
                _lfoFrequency = value;
                _lfo.SetParameter("freq", value);
            }
        }
        private float _lfoFrequency = 1;

        /// <summary>
        /// Gets or sets LFO signal generator.
        /// </summary>
        public SignalBuilder Lfo
        {
            get => _lfo;
            set
            {
                _lfo = value;
                _lfo.SetParameter("min", 0.0).SetParameter("max", 1.0);
            }
        }
        private SignalBuilder _lfo;

        /// <summary>
        /// Gets or sets interpolation mode.
        /// </summary>
        public InterpolationMode InterpolationMode
        {
            get => _delayLine.InterpolationMode;
            set => _delayLine.InterpolationMode = value;
        }

        /// <summary>
        /// Construct <see cref="VibratoEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
        /// <param name="width">Width (in seconds)</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
        public VibratoEffect(int samplingRate,
                             float lfoFrequency = 1/*Hz*/,
                             float width = 0.003f/*sec*/,
                             InterpolationMode interpolationMode = InterpolationMode.Linear,
                             float reserveWidth = 0/*sec*/)

            : this(samplingRate, new SineBuilder().SampledAt(samplingRate), width, interpolationMode, reserveWidth)
        {
            LfoFrequency = lfoFrequency;
        }

        /// <summary>
        /// Construct <see cref="VibratoEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="width">Width (in seconds)</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveWidth">Max width (in seconds) for reserving the size of delay line</param>
        public VibratoEffect(int samplingRate,
                             SignalBuilder lfo,
                             float width = 0.003f/*sec*/,
                             InterpolationMode interpolationMode = InterpolationMode.Linear,
                             float reserveWidth = 0/*sec*/)
        {
            _fs = samplingRate;
            _width = width;

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
        /// Process one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var delay = _lfo.NextSample() * _width * _fs;

            var delayedSample = _delayLine.Read(delay);

            _delayLine.Write(sample);

            return Dry * sample + Wet * delayedSample;
        }

        /// <summary>
        /// Reset effect.
        /// </summary>
        public override void Reset()
        {
            _delayLine.Reset();
            _lfo.Reset();
        }
    }
}
