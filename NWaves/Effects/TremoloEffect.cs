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
        /// Depth
        /// </summary>
        public float Depth { get; set; }
        
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
        private float _index;
        public float Index
        {
            get => _index;
            set
            {
                _index = value;
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
        /// <param name="depth"></param>
        /// <param name="frequency"></param>
        /// <param name="tremoloIndex"></param>
        public TremoloEffect(int samplingRate, float depth = 0.5f, float frequency = 10/*Hz*/, float tremoloIndex = 0.5f)
        {
            Lfo = new CosineBuilder().SampledAt(samplingRate);

            Depth = depth;
            Frequency = frequency;
            Index = tremoloIndex;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lfo"></param>
        /// <param name="depth"></param>
        public TremoloEffect(SignalBuilder lfo, float depth = 0.5f)
        {
            Lfo = lfo;
            Depth = depth;
        }

        /// <summary>
        /// Method implements simple tremolo effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = sample * (1 - Depth + Depth * Lfo.NextSample());

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
