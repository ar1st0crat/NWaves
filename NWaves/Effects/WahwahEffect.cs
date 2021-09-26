using System;
using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;

namespace NWaves.Effects
{
    /// <summary>
    /// Class representing Wah-Wah audio effect.
    /// </summary>
    public class WahwahEffect : AudioEffect
    {
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
        /// Gets or sets Q factor (a.k.a. Quality Factor, resonance).
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Gets or sets LFO signal generator.
        /// </summary>
        public SignalBuilder Lfo { get; set; }

        /// <summary>
        /// Sampling rate.
        /// </summary>
        private readonly int _fs;

        /// <summary>
        /// Construct <see cref="WahwahEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfoFrequency">LFO frequency (in Hz)</param>
        /// <param name="minFrequency">Minimal LFO frequency (in Hz)</param>
        /// <param name="maxFrequency">Maximal LFO frequency (in Hz)</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        public WahwahEffect(int samplingRate,
                            float lfoFrequency = 1.0f,
                            float minFrequency = 300,
                            float maxFrequency = 1500,
                            float q = 0.5f)
        {
            _fs = samplingRate;

            Lfo = new TriangleWaveBuilder().SampledAt(samplingRate);

            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;
        }

        /// <summary>
        /// Construct <see cref="WahwahEffect"/> from <paramref name="lfo"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfo">LFO signal generator</param>
        /// <param name="q">Q factor (a.k.a. Quality Factor, resonance)</param>
        public WahwahEffect(int samplingRate, SignalBuilder lfo, float q = 0.5f)
        {
            _fs = samplingRate;
            Q = q;
            Lfo = lfo;
        }

        /// <summary>
        /// Process one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var fs2pi = 2 * Math.PI / _fs;

            var f = (float)(2 * Math.Sin(Lfo.NextSample() * fs2pi));

            _yh = sample - _yl - Q * _yb;
            _yb += f * _yh;
            _yl += f * _yb;

            return _yb * Wet + sample * Dry;
        }

        /// <summary>
        /// Reset effect.
        /// </summary>
        public override void Reset()
        {
            _yh = _yb = _yl = 0;
            Lfo.Reset();
        }

        private float _yh, _yb, _yl;
    }
}
