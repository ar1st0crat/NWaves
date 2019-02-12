using NWaves.Filters.Base;
using NWaves.Signals;
using System.Linq;

namespace NWaves.Effects
{
    /// <summary>
    /// Audio effect
    /// </summary>
    public abstract class AudioEffect : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Wet
        /// </summary>
        public float Wet { get; set; } = 1f;

        /// <summary>
        /// Dry
        /// </summary>
        public float Dry { get; set; } = 0f;

        /// <summary>
        /// Online processing
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public abstract float Process(float sample);

        /// <summary>
        /// Reset effect
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Offline processing
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public virtual DiscreteSignal ApplyTo(DiscreteSignal signal,
                                              FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
