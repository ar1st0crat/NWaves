using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for echo effect
    /// </summary>
    public class EchoEffect : IFilter
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
        public EchoEffect(double length, double decay)
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
            var m = (int)(Length * signal.SamplingRate);
            var kernel = new double [m + 1];
            kernel[0] = 1.0;
            kernel[m] = Decay;

            var delayFilter = new FirFilter(kernel);

            return delayFilter.ApplyTo(signal);
        }
    }
}
