using NWaves.Filters.BiQuad;
using NWaves.Signals.Builders;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for phaser effect
    /// </summary>
    public class PhaserEffect : AudioEffect
    {
        /// <summary>
        /// Q
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// LFO frequency
        /// </summary>
        public float LfoFrequency { set { Lfo.SetParameter("freq", value); } }

        /// <summary>
        /// Min LFO frequency
        /// </summary>
        public float MinFrequency { set { Lfo.SetParameter("min", value); } }

        /// <summary>
        /// Max LFO frequency
        /// </summary>
        public float MaxFrequency { set { Lfo.SetParameter("max", value); } }

        /// <summary>
        /// LFO
        /// </summary>
        public SignalBuilder Lfo { get; set; }

        /// <summary>
        /// Sampling rate
        /// </summary>
        private int _fs;

        /// <summary>
        /// Notch filter with varying center frequency
        /// </summary>
        private NotchFilter _filter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="q"></param>
        /// <param name="lfoFrequency"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        public PhaserEffect(int samplingRate,
                            float lfoFrequency = 1.0f,
                            float minFrequency = 300,
                            float maxFrequency = 3000,
                            float q = 0.5f)
        {
            _fs = samplingRate;
            
            Lfo = new TriangleWaveBuilder().SampledAt(samplingRate);
            
            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;

            _filter = new NotchFilter(Lfo.NextSample() / _fs, Q);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfo"></param>
        /// <param name="q"></param>
        public PhaserEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)
        {
            _fs = samplingRate;
            Q = q;
            Lfo = lfo;

            _filter = new NotchFilter(Lfo.NextSample() / _fs, Q);
        }

        /// <summary>
        /// Method implements simple phaser effect
        /// </summary>
        /// <param name="sample">Input sample</param>
        /// <returns>Output sample</returns>
        public override float Process(float sample)
        {
            var output = _filter.Process(sample);

            _filter.Change(Lfo.NextSample() / _fs, Q);     // vary notch filter coefficients

            return output * Wet + sample * Dry;
        }

        public override void Reset()
        {
            _filter.Reset();
            Lfo.Reset();
        }
    }
}
