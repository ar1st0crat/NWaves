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
        private float _length;
        public float Length
        {
            get => _length;
            set
            {
                _length = value;
                _filter = new CombFeedbackFilter((int)(value * _fs), am: _decay);
            }
        }

        /// <summary>
        /// Decay
        /// </summary>
        private float _decay;
        public float Decay
        {
            get => _decay;
            set
            {
                _decay = value;
                _filter.Change(1, _decay);
            }
        }

        /// <summary>
        /// Feedforward comb filter
        /// </summary>
        private CombFeedbackFilter _filter;

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _fs;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="length"></param>
        /// <param name="decay"></param>
        public EchoEffect(int samplingRate, float length, float decay)
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
