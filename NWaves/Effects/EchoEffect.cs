using System;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for echo effect.
    /// Essentially it's a feedback comb filter.
    /// </summary>
    public class EchoEffect : IFilter
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
        /// Constructor
        /// </summary>
        /// <param name="length"></param>
        /// <param name="decay"></param>
        public EchoEffect(float length, float decay)
        {
            Length = length;
            Decay = decay;
        }

        /// <summary>
        /// Method implements simple echo effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var delay = (int)(Length * signal.SamplingRate);
            var delayFilter = new CombFeedbackFilter(delay, am: Decay);
            return delayFilter.ApplyTo(signal);
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