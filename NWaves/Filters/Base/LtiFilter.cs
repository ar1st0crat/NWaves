using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Base class for all kinds of LTI filters.
    /// Provides general algorithms for computing impulse and frequency responses
    /// and leaves method ApplyTo() abstract.
    /// </summary>
    public abstract class LtiFilter : IFilter
    {
        /// <summary>
        /// The filtering algorithm that should be implemented by particular subclass
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="filteringOptions">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public abstract DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringOptions filteringOptions = FilteringOptions.Auto);

        /// <summary>
        /// Zeros of the transfer function
        /// </summary>
        public abstract ComplexDiscreteSignal Zeros { get; set; }

        /// <summary>
        /// Poles of the transfer function
        /// </summary>
        public abstract ComplexDiscreteSignal Poles { get; set; }

        /// <summary>
        /// Returns the real-valued impulse response of a filter.
        /// </summary>
        /// <param name="length">
        /// The length of an impulse reponse.
        /// If the filter is IIR, then it's the length of truncated infinite impulse reponse.
        /// </param>
        public abstract double[] ImpulseResponse(int length = 512);

        /// <summary>
        /// Returns the complex frequency response of a filter.
        /// 
        /// Method calculates the Frequency Response of a filter
        /// by taking FFT of an impulse response (possibly truncated).
        /// </summary>
        /// <param name="length">Number of frequency response samples</param>
        public virtual ComplexDiscreteSignal FrequencyResponse(int length = 512)
        {
            var real = ImpulseResponse(length);
            var imag = new double[length];

            var fft = new Fft64(length);
            fft.Direct(real, imag);

            return new ComplexDiscreteSignal(1, real.Take(length / 2 + 1),
                                                imag.Take(length / 2 + 1));
        }
    }
}
