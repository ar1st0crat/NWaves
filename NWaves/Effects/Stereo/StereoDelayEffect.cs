using NWaves.Utils;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Stereo delay effect
    /// </summary>
    public class StereoDelayEffect : StereoEffect
    {
        /// <summary>
        /// Left channel delay effect
        /// </summary>
        private readonly DelayEffect _delayEffectLeft;

        /// <summary>
        /// Right channel delay effect
        /// </summary>
        private readonly DelayEffect _delayEffectRight;

        /// <summary>
        /// Left channel delay (in seconds)
        /// </summary>
        public float DelayLeft
        {
            get => _delayEffectLeft.Delay;
            set => _delayEffectLeft.Delay = value;
        }

        /// <summary>
        /// Right channel delay (in seconds)
        /// </summary>
        public float DelayRight
        {
            get => _delayEffectRight.Delay;
            set => _delayEffectRight.Delay = value;
        }

        /// <summary>
        /// Left channel feedback
        /// </summary>
        public float FeedbackLeft
        {
            get => _delayEffectLeft.Feedback;
            set => _delayEffectLeft.Feedback = value;
        }

        /// <summary>
        /// Right channel feedback
        /// </summary>
        public float FeedbackRight
        {
            get => _delayEffectRight.Feedback;
            set => _delayEffectRight.Feedback = value;
        }

        /// <summary>
        /// Pan
        /// </summary>
        public float Pan { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="pan"></param>
        /// <param name="delayLeft"></param>
        /// <param name="feedbackLeft"></param>
        /// <param name="delayRight"></param>
        /// <param name="feedbackRight"></param>
        /// <param name="interpolationMode"></param>
        /// <param name="reserveDelay"></param>
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

        public override void Process(ref float left, ref float right)
        {
            var delayedLeft = _delayEffectLeft.Process(left);
            var delayedRight = _delayEffectRight.Process(right);

            delayedLeft *= 1 - Pan;
            delayedRight *= Pan;

            left = left * Dry + delayedLeft * Wet;
            right = right * Dry + delayedRight * Wet;
        }

        public override void Reset()
        {
            _delayEffectLeft.Reset();
            _delayEffectRight.Reset();
        }
    }
}
