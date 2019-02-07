using NWaves.Filters;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for delay effect.
    /// Essentially it's a feedforward comb filter.
    /// </summary>
    public class DelayEffect : AudioEffect
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
        private CombFeedforwardFilter _filter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="length"></param>
        /// <param name="decay"></param>
        public DelayEffect(int samplingRate, float length, float decay)
        {
            Length = length;
            Decay = decay;

            _filter = new CombFeedforwardFilter((int)(length * samplingRate), bm: decay);
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
