using NWaves.Effects.Base;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Delay effect
    /// </summary>
    public class DelayEffect : AudioEffect
    {
        /// <summary>
        /// Delay line
        /// </summary>
        private readonly FractionalDelayLine _delayLine;

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Delay (in seconds)
        /// </summary>
        private float _delay;
        public float Delay
        {
            get => _delay / _fs;
            set
            {
                _delay = _fs * value;
                _delayLine.Ensure(_fs, value);
            }
        }

        /// <summary>
        /// Feedback coefficient
        /// </summary>
        public float Feedback { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="delay"></param>
        /// <param name="feedback"></param>
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

        public override float Process(float sample)
        {
            var delayed = _delayLine.Read(_delay);

            var output = sample + delayed * Feedback;

            _delayLine.Write(sample);

            return sample * Dry + output * Wet;
        }

        public override void Reset()
        {
            _delayLine.Reset();
        }
    }
}
