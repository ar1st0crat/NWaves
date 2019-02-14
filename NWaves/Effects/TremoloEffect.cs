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
        public float Frequency { set { Lfo.SetParameter("freq", value); } }

        /// <summary>
        /// Tremolo index (modulation index)
        /// </summary>
        public float TremoloIndex { set { Lfo.SetParameter("min", 0).SetParameter("max", value * 2); } }

        /// <summary>
        /// LFO
        /// </summary>
        public SignalBuilder Lfo { get; set; }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private int _fs;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="frequency"></param>
        /// <param name="tremoloIndex"></param>
        public TremoloEffect(int samplingRate, float frequency = 10/*Hz*/, float tremoloIndex = 0.5f)
        {
            _fs = samplingRate;
            
            Lfo = new CosineBuilder().SampledAt(samplingRate);

            Frequency = frequency;
            TremoloIndex = tremoloIndex;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfo"></param>
        public TremoloEffect(int samplingRate, SignalBuilder lfo)
        {
            _fs = samplingRate;
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

        public override void Reset()
        {
            Lfo.Reset();
        }
    }
}
