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
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public abstract float Process(float sample);

        /// <summary>
        /// Resets effect.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Applies effect to entire <paramref name="signal"/> and returns new processed signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public virtual DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);

        /// <summary>
        /// Maximum number of interleaved channels in an audio buffer
        /// </summary>
        public const int MAX_CHANNELS = 16; //                                                                    2022-04-20 J.P.B.

    }
}
