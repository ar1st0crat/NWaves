using NWaves.Utils;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Stereo ping pong delay effect
    /// </summary>
    public class PingPongDelayEffect : StereoEffect
    {
        /// <summary>
        /// Left channel delay line
        /// </summary>
        private readonly FractionalDelayLine _delayLineLeft;

        /// <summary>
        /// Righ channel delay line
        /// </summary>
        private readonly FractionalDelayLine _delayLineRight;

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
                _delayLineLeft.Ensure(_fs, value);
                _delayLineRight.Ensure(_fs, value);
                _delay = _fs * value;
            }
        }

        /// <summary>
        /// Feedback coefficient
        /// </summary>
        public float Feedback { get; set; }

        /// <summary>
        /// Pan
        /// </summary>
        public float Pan { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="delay"></param>
        /// <param name="feedback"></param>
        /// <param name="balance"></param>
        /// <param name="reserveDelay"></param>
        public PingPongDelayEffect(int samplingRate,
                                   float pan,
                                   float delay,
                                   float feedback = 0.5f,
                                   InterpolationMode interpolationMode = InterpolationMode.Nearest,
                                   float reserveDelay = 0/*sec*/)
        {
            _fs = samplingRate;

            if (reserveDelay < delay)
            {
                _delayLineLeft = new FractionalDelayLine(samplingRate, delay, interpolationMode);
                _delayLineRight = new FractionalDelayLine(samplingRate, delay, interpolationMode);
            }
            else
            {
                _delayLineLeft = new FractionalDelayLine(samplingRate, reserveDelay, interpolationMode);
                _delayLineRight = new FractionalDelayLine(samplingRate, reserveDelay, interpolationMode);
            }

            Delay = delay;
            Feedback = feedback;
            Pan = pan;
        }

        public override void Process(ref float left, ref float right)
        {
            var delayedLeft = _delayLineLeft.Read(_delay);
            var delayedRight = _delayLineRight.Read(_delay);

            var processedLeft = left * (1 - Pan) + delayedRight * Feedback;
            var processedRight = right * Pan + delayedLeft * Feedback;

            _delayLineLeft.Write(processedLeft);
            _delayLineRight.Write(processedRight);

            left = left * Dry + processedLeft * Wet;
            right = right * Dry + processedRight * Wet;
        }

        public override void Reset()
        {
            _delayLineLeft.Reset();
            _delayLineRight.Reset();
        }
    }
}
