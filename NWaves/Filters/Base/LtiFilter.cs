using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Base class for all kinds of filters.
    /// Provides general algorithms for computing impulse and frequency responses
    /// and leaves method ApplyTo() abstract.
    /// </summary>
    public abstract class LtiFilter : IFilter
    {
        /// <summary>
        /// Default length of truncated impulse reponse
        /// </summary>
        protected const int DefaultImpulseResponseLength = 512;
        
        /// <summary>
        /// The filtering algorithm that should be implemented by particular subclass
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="filteringOptions">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public abstract DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringOptions filteringOptions = FilteringOptions.Auto);

        /// <summary>
        /// The length of truncated infinite impulse reponse
        /// </summary>
        public int ImpulseResponseLength { get; set; }

        /// <summary>
        /// Returns the complex frequency response of a filter.
        /// 
        /// Method calculates the Frequency Response of a filter
        /// by taking FFT of truncated impulse response.
        /// </summary>
        public virtual ComplexDiscreteSignal FrequencyResponse
        {
            get
            {
                var real = ImpulseResponse.Samples;
                var imag = new double[ImpulseResponseLength];

                Transform.Fft(real, imag, ImpulseResponseLength);

                return new ComplexDiscreteSignal(1, real, imag);
            }
        }

        /// <summary>
        /// Returns the real-valued impulse response of a filter.
        /// 
        /// Method calculates the Impulse Response of a filter
        /// by feeding the unit impulse into it.
        /// </summary>
        public virtual DiscreteSignal ImpulseResponse
        {
            get
            {
                var impulse = new DiscreteSignal(1, ImpulseResponseLength) { [0] = 1.0 };
                return ApplyTo(impulse);
            }
        }
    }
}
