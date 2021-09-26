using NWaves.Utils;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Class representing stereo delay audio effect.
    /// </summary>
    public class StereoDelayEffect : StereoEffect
    {
        /// <summary>
        /// Left channel delay effect.
        /// </summary>
        private readonly DelayEffect _delayEffectLeft;

        /// <summary>
        /// Right channel delay effect.
        /// </summary>
        private readonly DelayEffect _delayEffectRight;

        /// <summary>
        /// Gets or sets left channel delay (in seconds).
        /// </summary>
        public float DelayLeft
        {
            get => _delayEffectLeft.Delay;
            set => _delayEffectLeft.Delay = value;
        }

        /// <summary>
        /// Gets or sets right channel delay (in seconds).
        /// </summary>
        public float DelayRight
        {
            get => _delayEffectRight.Delay;
            set => _delayEffectRight.Delay = value;
        }

        /// <summary>
        /// Gets or sets left channel feedback.
        /// </summary>
        public float FeedbackLeft
        {
            get => _delayEffectLeft.Feedback;
            set => _delayEffectLeft.Feedback = value;
        }

        /// <summary>
        /// Gets or sets right channel feedback.
        /// </summary>
        public float FeedbackRight
        {
            get => _delayEffectRight.Feedback;
            set => _delayEffectRight.Feedback = value;
        }

        /// <summary>
        /// Gets or sets pan.
        /// </summary>
        public float Pan { get; set; }

        /// <summary>
        /// Construct <see cref="StereoDelayEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="pan">Pan</param>
        /// <param name="delayLeft">Left channel delay (in seconds)</param>
        /// <param name="feedbackLeft">Left channel feedback</param>
        /// <param name="delayRight">Right channel delay (in seconds)</param>
        /// <param name="feedbackRight">Right channel feedback</param>
        /// <param name="interpolationMode">Interpolation mode for fractional delay line</param>
        /// <param name="reserveDelay">Max delay for reserving the size of delay line</param>
        public StereoDelayEffect(int samplingRate,
                                 float pan,
                                 float delayLeft,
                                 float delayRight,
                                 float feedbackLeft = 0.5f,
                                 float feedbackRight = 0.5f,
                                 InterpolationMode interpolationMode = InterpolationMode.Nearest,
                                 float reserveDelay = 0/*sec*/)
        {
            _delayEffectLeft = new DelayEffect(samplingRate, delayLeft, feedbackLeft, interpolationMode, reserveDelay);
            _delayEffectRight = new DelayEffect(samplingRate, delayRight, feedbackRight, interpolationMode, reserveDelay);
            
            Pan = pan;
        }

        /// <summary>
        /// Process one sample in each of two channels : [ input left , input right ] -> [ output left , output right ].
        /// </summary>
        /// <param name="left">Input sample in left channel</param>
        /// <param name="right">Input sample in right channel</param>
        public override void Process(ref float left, ref float right)
        {
            var delayedLeft = _delayEffectLeft.Process(left);
            var delayedRight = _delayEffectRight.Process(right);

            delayedLeft *= 1 - Pan;
            delayedRight *= Pan;

            left = left * Dry + delayedLeft * Wet;
            right = right * Dry + delayedRight * Wet;
        }

        /// <summary>
        /// Reset stereo delay effect.
        /// </summary>
        public override void Reset()
        {
            _delayEffectLeft.Reset();
            _delayEffectRight.Reset();
        }
    }
}
