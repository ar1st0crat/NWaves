using NWaves.Effects.Base;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Bit crusher effect
    /// </summary>
    public class BitCrusherEffect : AudioEffect
    {
        /// <summary>
        /// Step is calculated from bit depth
        /// </summary>
        private float _step;

        /// <summary>
        /// Number of bits
        /// </summary>
        private int _bitDepth;
        public int BitDepth 
        {
            get => _bitDepth;
            set
            {
                _bitDepth = value;
                _step = 2 * (float)Math.Pow(0.5, _bitDepth);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bitDepth"></param>
        public BitCrusherEffect(int bitDepth)
        {
            BitDepth = bitDepth;
        }

        public override float Process(float sample)
        {
            var output = (float)(_step * Math.Floor(sample / _step + 0.5));

            return output * Wet + sample * Dry;
        }

        public override void Reset()
        {
        }
    }
}
