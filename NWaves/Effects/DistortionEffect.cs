using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for distortion effect.
    /// DAFX book [Udo Zoelzer], p.124-125.
    /// </summary>
    public class DistortionEffect : AudioEffect
    {
        /// <summary>
        /// Amount of distortion
        /// </summary>
        public float Gain { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain"></param>
        public DistortionEffect(float gain = 20.0f)
        {
            Gain = gain;
        }

        /// <summary>
        /// Method implements simple distortion effect
        /// </summary>
        /// <param name="sample">Input sample</param>
        /// <returns>Output sample</returns>
        public override float Process(float sample)
        {
            var q = Gain * sample;
            var output = Math.Sign(q) * (1 - Math.Exp(-Math.Abs(q)));
            return (float)(output * Wet + sample * Dry);
        }

        public override void Reset()
        {
        }
    }
}
