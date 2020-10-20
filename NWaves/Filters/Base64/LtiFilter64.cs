using NWaves.Filters.Base;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Base class for all kinds of LTI filters (double precision).
    /// 
    /// Provides abstract TransferFunction property
    /// and leaves methods ApplyTo() and Process() abstract.
    /// </summary>
    public abstract class LtiFilter64 : IFilter64, IOnlineFilter64
    {
        /// <summary>
        /// Transfer function
        /// </summary>
        public abstract TransferFunction Tf { get; protected set; }

        /// <summary>
        /// The offline filtering algorithm that should be implemented by particular subclass
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="method">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public abstract double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto);

        /// <summary>
        /// The online filtering algorithm should be implemented by particular subclass
        /// </summary>
        /// <param name="input">Input sample</param>
        /// <returns>Output sample</returns>
        public abstract double Process(double input);

        /// <summary>
        /// Reset filter (clear all internal buffers)
        /// </summary>
        public abstract void Reset();
    }
}
