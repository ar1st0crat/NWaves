using NWaves.Effects.Base;
using NWaves.Signals.Builders;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for tremolo effect
    /// </summary>
    public class TremoloEffect : AudioEffect
    {
        /// <summary>
        /// Modulation frequency
        /// </summary>
        private float _frequency;
        public float Frequency
        {
            get => _frequency;
            set
            {
                _frequency = value;
                Lfo.SetParameter("freq", value);
            }
        }

        /// <summary>
        /// Tremolo index (modulation index)
        /// </summary>
        private float _tremoloIndex;
        public float TremoloIndex
        {
            get => _tremoloIndex;
            set
            {
                _tremoloIndex = value;
                Lfo.SetParameter("min", 0).SetParameter("max", value * 2);
            }
        }

        /// <summary>
        /// LFO
        /// </summary>
        public SignalBuilder Lfo { get; set; }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="frequency"></param>
        /// <param name="tremoloIndex"></param>
        public TremoloEffect(int samplingRate, float frequency = 10/*Hz*/, float tremoloIndex = 0.5f)
        {
            Lfo = new CosineBuilder().SampledAt(samplingRate);

            Frequency = frequency;
            TremoloIndex = tremoloIndex;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lfo"></param>
        public TremoloEffect(SignalBuilder lfo)
        {
            Lfo = lfo;
        }

        /// <summary>
        /// Method implements simple tremolo effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = sample * Lfo.NextSample();
            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public override void Reset()
        {
            Lfo.Reset();
        }
    }
}
