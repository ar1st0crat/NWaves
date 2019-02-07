using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for overdrive effect.
    /// DAFX book [Udo Zoelzer], p.118.
    /// </summary>
    public class OverdriveEffect : AudioEffect
    {
        /// <summary>
        /// Input gain
        /// </summary>
        public float InputGain { get; }

        /// <summary>
        /// Output gain
        /// </summary>
        public float OutputGain { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain"></param>
        public OverdriveEffect(float inputGain, float outputGain = 0.4f)
        {
            InputGain = inputGain;
            OutputGain = outputGain;
        }

        /// <summary>
        /// Method implements simple overdrive effect
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var lowerThreshold = 0.33333f;
            var upperThreshold = 0.66667f;

            var abs = Math.Abs(sample) * InputGain;

            float output;

            if (abs > upperThreshold)
            {
                output = Math.Sign(sample);
            }
            else if (abs >= lowerThreshold)
            {
                output = Math.Sign(sample) * (3 - (2 - 3 * abs) * (2 - 3 * abs)) / 3;
            }
            else
            {
                output = 2 * sample;
            }

            output *= OutputGain;

            return output * Wet + sample * Dry;
        }

        public override void Reset()
        {
        }
    }
}
