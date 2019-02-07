using NWaves.Filters.Base;
using NWaves.Signals;

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
            var output = signal.Copy();

            for (var i = 0; i < signal.Length; i++)
            {
                output[i] = Process(signal[i]);
            }

            return output;
        }
    }
}
