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
        /// Delay length (in seconds)
        /// </summary>
        public float Length { set { _filter = new CombFeedforwardFilter((int)(value * _fs), bm: _decay); } }

        /// <summary>
        /// Decay
        /// </summary>
        private float _decay;
        public float Decay
        {
            get { return _decay; }
            set
            {
                _decay = value;
                _filter.Change(1, _decay);
            }
        }

        /// <summary>
        /// Feedforward comb filter
        /// </summary>
        private CombFeedforwardFilter _filter;

        /// <summary>
        /// Sampling rate
        /// </summary>
        private int _fs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="length"></param>
        /// <param name="decay"></param>
        public DelayEffect(int samplingRate, float length, float decay)
        {
            _fs = samplingRate;
            Length = length;
            Decay = decay;
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
