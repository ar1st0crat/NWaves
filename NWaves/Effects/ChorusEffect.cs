using NWaves.Effects.Base;
using NWaves.Signals.Builders;
using NWaves.Signals.Builders.Base;
using NWaves.Utils;
using System.Linq;

namespace NWaves.Effects
{
    /// <summary>
    /// Chorus effect.
    /// 
    /// Currently the implementation is not very efficient:
    /// it's just a set of vibrato effects.
    /// 
    /// </summary>
    public class ChorusEffect : AudioEffect
    {
        /// <summary>
        /// Widths for each voice (max delays in seconds)
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
        /// LFO frequencies
        /// </summary>
        private float[] _lfoFrequencies;
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

        /// <summary>
        /// Chorus voices
        /// </summary>
        private readonly VibratoEffect[] _voices;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfoFrequencies"></param>
        /// <param name="widths"></param>
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
        /// Constructor with LFOs
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="lfos"></param>
        /// <param name="widths"></param>
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
        /// Process sample
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var chorus = _voices.Sum(v => v.Process(sample)) / _voices.Length;

            return sample * Dry + chorus * Wet;
        }

        /// <summary>
        /// Reset effect
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
