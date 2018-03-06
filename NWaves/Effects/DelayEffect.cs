using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for delay effect.
    /// Essentially it's a feedforward comb filter.
    /// </summary>
    public class DelayEffect : IFilter
    {
        /// <summary>
        /// Echo length (in seconds)
        /// </summary>
        public double Length { get; }

        /// <summary>
        /// Decay
        /// </summary>
        public double Decay { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length"></param>
        /// <param name="decay"></param>
        public DelayEffect(double length, double decay)
        {
            Length = length;
            Decay = decay;
        }

        /// <summary>
        /// Method implements simple delay effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var delay = (int)(Length * signal.SamplingRate);
            var delayFilter = new CombFeedforwardFilter(delay, bm: Decay);
            return delayFilter.ApplyTo(signal);
        }
    }
}
