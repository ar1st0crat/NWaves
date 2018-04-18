using System;
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
        public float Length { get; }

        /// <summary>
        /// Decay
        /// </summary>
        public float Decay { get; }

        /// <summary>
        /// Delay filter
        /// </summary>
        private CombFeedforwardFilter _delayFilter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="length"></param>
        /// <param name="decay"></param>
        public DelayEffect(float length, float decay)
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
            _delayFilter = new CombFeedforwardFilter(delay, bm: Decay);
            return _delayFilter.ApplyTo(signal);
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
        /// 
        /// </summary>
        public void Reset()
        {
        }
    }
}
