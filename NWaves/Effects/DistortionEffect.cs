using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for distortion effect
    /// </summary>
    public class DistortionEffect : IFilter
    {
        /// <summary>
        /// Method implements simple distortion effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var output = signal.Samples.Select(s => s > 0 ? 1 - Math.Exp(-s) : -1 + Math.Exp(s));

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
