using NWaves.Effects.Base;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Class representing Delay audio effect.
    /// </summary>
    public class DelayEffect : AudioEffect
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
        /// Gets or sets delay (in seconds).
        /// </summary>
        public float Delay
        {
            get => _delay / _fs;
            set
            {
                _delayLine.Ensure(_fs, value);
                _delay = _fs * value;
            }
        }
        private float _delay;

        /// <summary>
        /// Gets or sets feedback parameter.
        /// </summary>
        public float Feedback { get; set; }

        /// <summary>
        /// Construct <see cref="DelayEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="delay">Delay (in seconds)</param>
        /// <param name="feedback">Feedback</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
        public DelayEffect(int samplingRate,
                           float delay,
                           float feedback = 0.5f,
                           InterpolationMode interpolationMode = InterpolationMode.Nearest,
                           float reserveDelay = 0f)
        {
            _fs = samplingRate;

            if (reserveDelay < delay)
            {
                _delayLine = new FractionalDelayLine(samplingRate, delay, interpolationMode);
            }
            else
            {
                _delayLine = new FractionalDelayLine(samplingRate, reserveDelay, interpolationMode);
            }

            Delay = delay;
            Feedback = feedback;
        }

        /// <summary>
        /// Process one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var delayed = _delayLine.Read(_delay);

            var output = sample + delayed * Feedback;

            _delayLine.Write(sample);

            return sample * Dry + output * Wet;
        }

        /// <summary>
        /// Reset effect.
        /// </summary>
        public override void Reset()
        {
            _delayLine.Reset();
        }
    }
}
