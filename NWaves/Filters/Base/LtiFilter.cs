using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Abstract class for Linear Time-Invariant (LTI) filters.
    /// </summary>
    public abstract class LtiFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Gets transfer function of LTI filter.
        /// </summary>
        public abstract TransferFunction Tf { get; protected set; }

        // NOTE.
        //
        // TF is made abstract as of ver.0.9.2 to allow subclasses using memory more efficiently.
        // It's supposed that subclasses will generate TransferFunction object on the fly from filter coeffs
        // OR aggregate it in internal field (only if it was set specifically from outside).
        // 
        // The example of the latter case is when we really need double precision for FDA
        // or when TF was generated from precomputed poles and zeros.
        // 
        // The general rule is:
        // 
        //      * Use LtiFilter subclasses for FILTERING;
        //      * Use TransferFunction class for FILTER DESIGN AND ANALYSIS.
        //

        /// <summary>
        /// Applies LTI filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public abstract DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto);

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public abstract float Process(float sample);

        /// <summary>
        /// Resets LTI filter.
        /// </summary>
        public abstract void Reset();
    }
}
