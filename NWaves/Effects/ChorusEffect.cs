using NWaves.Effects.Base;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;
using System.Linq;

namespace NWaves.Effects
{
    // Currently, the implementation is not very efficient:
    // it's just a set of vibrato effects.
 
    /// <summary>
    /// Represents Chorus audio effect.
    /// </summary>
    public class ChorusEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets widths for each voice (max delays in seconds).
        /// </summary>
        public float[] Widths
        {
            get => _voices.Select(v => v.Width).ToArray();
            set
            {
                for (var i = 0; i < _voices.Length; i++)
                {
                    _voices[i].Width = value[i];
                }
            }
        }

        /// <summary>
        /// Gets or sets LFO frequencies for each voice.
        /// </summary>
        public float[] LfoFrequencies
        {
            get => _lfoFrequencies;
            set
            {
                _lfoFrequencies = value;

                for (var i = 0; i < _voices.Length; i++)
                {
                    _voices[i].LfoFrequency = value[i];
                }
            }
        }
        private float[] _lfoFrequencies;

        /// <summary>
        /// Chorus voices.
        /// </summary>
        private readonly VibratoEffect[] _voices;

        /// <summary>
        /// Constructs <see cref="ChorusEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfoFrequencies">LFO frequencies for each voice</param>
        /// <param name="widths">Widths (max delays, in seconds) for each voice</param>
        public ChorusEffect(int samplingRate, float[] lfoFrequencies, float[] widths)
        {
            Guard.AgainstInequality(lfoFrequencies.Length, widths.Length, "Size of frequency array", "size of widths array");

            _lfoFrequencies = lfoFrequencies;

            _voices = new VibratoEffect[widths.Length];

            for (var i = 0; i < _voices.Length; i++)
            {
                _voices[i] = new VibratoEffect(samplingRate, lfoFrequencies[i], widths[i]);
            }
        }

        /// <summary>
        /// Constructs <see cref="ChorusEffect"/> from <paramref name="lfos"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="lfos">LFOs (in the form of signal builders)</param>
        /// <param name="widths">Widths (max delays, in seconds) for each voice</param>
        public ChorusEffect(int samplingRate, SignalBuilder[] lfos, float[] widths)
        {
            Guard.AgainstInequality(lfos.Length, widths.Length, "Size of frequency array", "number of LFOs");

            _voices = new VibratoEffect[widths.Length];

            for (var i = 0; i < _voices.Length; i++)
            {
                _voices[i] = new VibratoEffect(samplingRate, lfos[i], widths[i]);
            }
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var chorus = _voices.Sum(v => v.Process(sample)) / _voices.Length;

            return sample * Dry + chorus * Wet;
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
            foreach (var voice in _voices)
            {
                voice.Reset();
            }
        }
    }
}
