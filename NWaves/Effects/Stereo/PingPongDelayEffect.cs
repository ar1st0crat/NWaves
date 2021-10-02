using NWaves.Utils;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Represents stereo ping-pong delay audio effect.
    /// </summary>
    public class PingPongDelayEffect : StereoEffect
    {
        /// <summary>
        /// Left channel delay line.
        /// </summary>
        private readonly FractionalDelayLine _delayLineLeft;

        /// <summary>
        /// Righ channel delay line.
        /// </summary>
        private readonly FractionalDelayLine _delayLineRight;

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Gets or sets pan.
        /// </summary>
        public float Pan { get; set; }

        /// <summary>
        /// Gets or sets delay (in seconds).
        /// </summary>
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
        private float _delay;

        /// <summary>
        /// Gets or sets feedback coefficient.
        /// </summary>
        public float Feedback { get; set; }

        /// <summary>
        /// Constructs <see cref="PingPongDelayEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="pan">Pan</param>
        /// <param name="delay">Delay (in seconds)</param>
        /// <param name="feedback">Feedback</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
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

        /// <summary>
        /// Processes one sample in each of two channels : [ input left , input right ] -> [ output left , output right ].
        /// </summary>
        /// <param name="left">Input sample in left channel</param>
        /// <param name="right">Input sample in right channel</param>
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

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            _delayLineLeft.Reset();
            _delayLineRight.Reset();
        }
    }
}
