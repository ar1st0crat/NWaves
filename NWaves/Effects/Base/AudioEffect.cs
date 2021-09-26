using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects.Base
{
    /// <summary>
    /// Abstract class for audio effects.
    /// </summary>
    public abstract class AudioEffect : WetDryMixer, IFilter, IOnlineFilter
    {
        /// <summary>
        /// Process one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public abstract float Process(float sample);

        /// <summary>
        /// Reset effect.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Process entire signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public virtual DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
