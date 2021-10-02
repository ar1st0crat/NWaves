using NWaves.Effects.Base;
using NWaves.Filters.BiQuad;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Phaser audio effect.
    /// </summary>
    public class PhaserEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets LFO frequency (in Hz).
        /// </summary>
        public float LfoFrequency
        {
            get => _lfoFrequency;
            set
            {
                _lfoFrequency = value;
                Lfo.SetParameter("freq", value);
            }
        }
        private float _lfoFrequency;

        /// <summary>
        /// Gets or sets minimal LFO frequency (in Hz).
        /// </summary>
        public float MinFrequency
        {
            get => _minFrequency;
            set
            {
                _minFrequency = value;
                Lfo.SetParameter("min", value);
            }
        }
        private float _minFrequency;

        /// <summary>
        /// Gets or sets maximal LFO frequency (in Hz).
        /// </summary>
        public float MaxFrequency
        {
            get => _maxFrequency;
            set
            {
                _maxFrequency = value;
                Lfo.SetParameter("max", value);
            }
        }
        private float _maxFrequency;

        /// <summary>
        /// Get or sets LFO signal generator.
        /// </summary>
        public SignalBuilder Lfo { get; set; }

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Notch filter with varying center frequency.
        /// </summary>
        private readonly NotchFilter _filter;

        /// <summary>
        /// Constructs <see cref="PhaserEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
        /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
        /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
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
        /// Constructs <see cref="PhaserEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        public PhaserEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)
        {
            _fs = samplingRate;
            Q = q;
            Lfo = lfo;

            _filter = new NotchFilter(Lfo.NextSample() / _fs, Q);
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = _filter.Process(sample);

            _filter.Change(Lfo.NextSample() / _fs, Q);     // vary notch filter coefficients

            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            _filter.Reset();
            Lfo.Reset();
        }
    }
}
