using System;
using NWaves.Effects.Base;
using NWaves.Filters.Base;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for tube distortion effect.
    /// DAFX book [Udo Zoelzer], p.123-124.
    /// </summary>
    public class TubeDistortionEffect : AudioEffect
    {
        /// <summary>
        /// Input gain
        /// </summary>
        private float _inputGain;
        public float InputGain
        {
            get => (float)Scale.ToDecibel(_inputGain);
            set => _inputGain = (float)Scale.FromDecibel(value);
        }

        /// <summary>
        /// Output gain
        /// </summary>
        private float _outputGain;
        public float OutputGain
        {
            get => (float)Scale.ToDecibel(_outputGain);
            set => _outputGain = (float)Scale.FromDecibel(value);
        }

        /// <summary>
        /// Work point.
        /// Controls the linearity of the transfer function for low input levels.
        /// More negative - more linear.
        /// </summary>
        public float Q { get; set; }

        /// <summary>
        /// Distortion's character.
        /// Higher number - harder distortion.
        /// </summary>
        public float Dist { get; set; }

        /// <summary>
        /// Filter coefficient (close to 1.0) defining placement of poles 
        /// in the HP filter that removes DC component.
        /// </summary>
        public float Rh { get; set; }

        /// <summary>
        /// Filter coefficient (in range (0, 1)) defining placement of pole 
        /// in the LP filter used to simulate capacitances in tube amplifier.
        /// </summary>
        public float Rl { get; set; }

        /// <summary>
        /// Internal filter for output signal 
        /// that combines HP and LP filters mentioned above
        /// </summary>
        private readonly LtiFilter _outputFilter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inputGain"></param>
        /// <param name="outputGain"></param>
        /// <param name="q"></param>
        /// <param name="dist"></param>
        /// <param name="rh"></param>
        /// <param name="rl"></param>
        public TubeDistortionEffect(float inputGain = 20/*dB*/,
                                    float outputGain = -12/*dB*/,
                                    float q = -0.2f,
                                    float dist = 5,
                                    float rh = 0.995f,
                                    float rl = 0.5f)
        {
            InputGain = inputGain;
            OutputGain = outputGain;

            Q = q;
            Dist = dist;
            Rh = rh;
            Rl = rl;

            var filter1 = new IirFilter(new[] { 1.0, -2, 1 }, new[] { 1.0, -2 * Rh, Rh * Rh });
            var filter2 = new IirFilter(new[] { 1.0 - Rl },   new[] { 1.0, -Rl });

            _outputFilter = filter1 * filter2;
        }

        /// <summary>
        /// Tube distortion
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            float output;

            var q = sample * _inputGain;

            if (Math.Abs(Q) < 1e-10)
            {
                output = Math.Abs(q - Q) < 1e-10 ? 1.0f / Dist : (float)(q / (1 - Math.Exp(-Dist * q)));
            }
            else
            {
                output = Math.Abs(q - Q) < 1e-10 ?
                           (float)(1.0 / Dist + Q / (1 - Math.Exp(Dist * Q))) :
                           (float)((q - Q) / (1 - Math.Exp(-Dist * (q - Q))) + Q / (1 - Math.Exp(Dist * Q)));
            }

            output = _outputFilter.Process(output) * _outputGain;
            
            return output * Wet + sample * Dry;
        }

        public override void Reset()
        {
            _outputFilter.Reset();
        }
    }
}