using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for distortion effect.
    /// DAFX book [Udo Zolzer], p.124-125.
    /// </summary>
    public class DistortionEffect : IFilter
    {
        /// <summary>
        /// Amount of distortion
        /// </summary>
        public double Gain { get; }

        /// <summary>
        /// Mix of original and distorted sound (1 - only distorted)
        /// </summary>
        public double Mix { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gain"></param>
        /// <param name="mix"></param>
        public DistortionEffect(double gain = 1.0, double mix = 0.5)
        {
            Gain = gain;
            Mix = mix;
        }

        /// <summary>
        /// Method implements simple distortion effect
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

            var tempZ = signal.Samples.Select(s =>
            {
                var q = Gain * s / maxAmp;
                return Math.Sign(q) * (1 - Math.Exp(-Math.Abs(q)));
            });

            var maxZ = tempZ.Max(z => Math.Abs(z));
            var tempY = tempZ.Zip(signal.Samples, (z, x) => Mix * z * maxAmp / maxZ + (1 - Mix) * x);
            var maxY = tempY.Max(y => Math.Abs(y));

            var output = tempY.Select(y => y * maxAmp / maxY).ToArray();

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
