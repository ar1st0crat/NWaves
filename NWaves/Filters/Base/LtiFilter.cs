using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Base class for all kinds of LTI filters.
    /// 
    /// Provides abstract TransferFunction property
    /// and leaves methods ApplyTo() and Process() abstract.
    /// </summary>
    public abstract class LtiFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Transfer function.
        /// 
        /// It's made abstract as of ver.0.9.2 to allow subclasses using memory more efficiently.
        /// It's supposed that subclasses will generate TransferFunction object on the fly from filter coeffs
        /// OR aggregate it in internal field (only if it was set specifically from outside).
        /// 
        /// The example of the latter case is when we really need double precision for FDA
        /// or when TF was generated from precomputed poles and zeros.
        /// 
        /// The general rule is:
        /// 
        /// "Use LtiFilter subclasses for FILTERING;
        ///  Use TransferFunction class for FILTER DESIGN AND ANALYSIS".
        ///  
        /// </summary>
        public abstract TransferFunction Tf { get; protected set; }

        /// <summary>
        /// The offline filtering algorithm that should be implemented by particular subclass
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="method">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public abstract DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto);

        /// <summary>
        /// The online filtering algorithm should be implemented by particular subclass
        /// </summary>
        /// <param name="input">Input sample</param>
        /// <returns>Output sample</returns>
        public abstract float Process(float input);

        /// <summary>
        /// Reset filter (clear all internal buffers)
        /// </summary>
        public abstract void Reset();
    }
}
