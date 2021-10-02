using NWaves.Filters.Base;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Abstract class for Linear Time-Invariant (LTI) filters (double precision).
    /// </summary>
    public abstract class LtiFilter64 : IFilter64, IOnlineFilter64
    {
        /// <summary>
        /// Gets transfer function of LTI filter.
        /// </summary>
        public abstract TransferFunction Tf { get; protected set; }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public abstract double Process(double sample);

        /// <summary>
        /// Resets LTI filter.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public abstract double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto);
    }
}
