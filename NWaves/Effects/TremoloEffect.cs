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
        public float Frequency { get; }

        /// <summary>
        /// Tremolo index (modulation index)
        /// </summary>
        public float TremoloIndex { get; }

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
            Frequency = frequency;
            TremoloIndex = tremoloIndex;

            _lfo = new CosineBuilder()
                            .SetParameter("amp", tremoloIndex)
                            .SetParameter("freq", frequency)
                            .SampledAt(samplingRate);
        }

        /// <summary>
        /// Method implements simple tremolo effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = sample * (1 + _lfo.NextSample());
            return output * Wet + sample * Dry;
        }

        public override void Reset()
        {
        }

        private SignalBuilder _lfo;
    }
}
