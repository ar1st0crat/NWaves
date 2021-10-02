using NWaves.Effects.Base;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Represents Bitcrusher (distortion) audio effect.
    /// </summary>
    public class BitCrusherEffect : AudioEffect
    {
        /// <summary>
        /// Step is calculated from bit depth.
        /// </summary>
        private float _step;

        /// <summary>
        /// Gets or sets the bit depth (number of bits).
        /// </summary>
        public int BitDepth 
        {
            get => _bitDepth;
            set
            {
                _bitDepth = value;
                _step = 2 * (float)Math.Pow(0.5, _bitDepth);
            }
        }
        private int _bitDepth;

        /// <summary>
        /// Constructs <see cref="BitCrusherEffect"/> with given <paramref name="bitDepth"/>.
        /// </summary>
        /// <param name="bitDepth">Bit depth (number of bits)</param>
        public BitCrusherEffect(int bitDepth)
        {
            BitDepth = bitDepth;
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = (float)(_step * Math.Floor(sample / _step + 0.5));

            return output * Wet + sample * Dry;
        }

        /// <summary>
        /// Resets effect.
        /// </summary>
        public override void Reset()
        {
        }
    }
}
