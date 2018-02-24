using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for wah-wah effect.
    /// </summary>
    public class WahwahEffect : IFilter
    {
        /// <summary>
        /// Method implements simple wah-wah effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            return signal.Copy();
        }
    }
}
