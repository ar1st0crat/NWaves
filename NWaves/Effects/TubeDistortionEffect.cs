using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for tube distortion effect.
    /// DAFX book [Udo Zoelzer], p.123-124.
    /// </summary>
    public class TubeDistortionEffect : IFilter
    {
        /// <summary>
        /// Amount of distortion
        /// </summary>
        public float Gain { get; }

        /// <summary>
        /// Mix of original and distorted sound (1 - only distorted)
        /// </summary>
        public float Mix { get; }

        /// <summary>
        /// Work point.
        /// Controls the linearity of the transfer function for low input levels.
        /// More negative - more linear.
        /// </summary>
        public float Q { get; }

        /// <summary>
        /// Distortion's character.
        /// Higher number - harder distortion.
        /// </summary>
        public float Dist { get; }

        /// <summary>
        /// Filter coefficient (close to 1.0) defining placement of poles 
        /// in the HP filter that removes DC component.
        /// </summary>
        public float Rh { get; }

        /// <summary>
        /// Filter coefficient (in range (0, 1)) defining placement of pole 
        /// in the LP filter used to simulate capacitances in tube amplifier.
        /// </summary>
        public float Rl { get; }

        /// <summary>
        /// Internal filter for output signal 
        /// that combines HP and LP filters mentioned above
        /// </summary>
        private readonly LtiFilter _outputFilter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="mix"></param>
        /// <param name="q"></param>
        /// <param name="dist"></param>
        /// <param name="rh"></param>
        /// <param name="rl"></param>
        public TubeDistortionEffect(float gain = 20.0f,
                                    float mix = 0.9f,
                                    float q = -0.2f,
                                    float dist = 5,
                                    float rh = 0.995f,
                                    float rl = 0.5f)
        {
            Gain = gain;
            Mix = mix;
            Q = q;
            Dist = dist;
            Rh = rh;
            Rl = rl;

            var filter1 = new IirFilter(new[] { 1.0, -2, 1 }, new[] { 1.0, -2 * Rh, Rh * Rh });
            var filter2 = new IirFilter(new[] { 1.0 - Rl },   new[] { 1.0, -Rl });

            _outputFilter = filter1 * filter2;
        }

        /// <summary>
        /// Method implements tube distortion effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var maxAmp = signal.Samples.Max(s => Math.Abs(s));

            if (Math.Abs(maxAmp) < 1e-10)
            {
                return signal.Copy();
            }

            IEnumerable<float> tempZ;

            if (Math.Abs(Q) < 1e-10)
            {
                tempZ = signal.Samples.Select(s =>
                {
                    var q = Gain * s / maxAmp;
                    return Math.Abs(q - Q) < 1e-10 ?
                           1.0f / Dist :
                           (float)(q / (1 - Math.Exp(-Dist * q)));
                });
            }
            else
            {
                tempZ = signal.Samples.Select(s =>
                {
                    var q = Gain * s / maxAmp;
                    return Math.Abs(q - Q) < 1e-10 ?
                           (float)(1.0 / Dist + Q / (1 - Math.Exp(Dist * Q))) :
                           (float)((q - Q) / (1 - Math.Exp(-Dist * (q - Q))) + Q / (1 - Math.Exp(Dist * Q)));
                });
            }

            var maxZ = tempZ.Max(z => Math.Abs(z));
            var tempY = tempZ.Zip(signal.Samples, (z, x) => Mix * z * maxAmp / maxZ + (1 - Mix) * x);

            var maxY = tempY.Max(y => Math.Abs(y));
            var output = tempY.Select(y => y * maxAmp / maxY);

            return _outputFilter.ApplyTo(new DiscreteSignal(signal.SamplingRate, output));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
        }
    }
}