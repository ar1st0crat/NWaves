using NWaves.Filters;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for echo effect.
    /// Essentially it's a feedback comb filter.
    /// </summary>
    public class EchoEffect : AudioEffect
    {
        /// <summary>
        /// Echo length (in seconds)
        /// </summary>
        public float Length { get; }

        /// <summary>
        /// Decay
        /// </summary>
        public float Decay { get; }

        /// <summary>
        /// Feedforward comb filter
        /// </summary>
        private CombFeedbackFilter _filter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="length"></param>
        /// <param name="decay"></param>
        public EchoEffect(int samplingRate, float length, float decay)
        {
            Length = length;
            Decay = decay;

            _filter = new CombFeedbackFilter((int)(length * samplingRate), am: decay);
        }

        public override float Process(float sample)
        {
            return _filter.Process(sample) * Wet + sample * Dry;
        }

        public override void Reset()
        {
            _filter.Reset();
        }
    }
}
