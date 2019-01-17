using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Base class for all kinds of LTI filters.
    /// Provides general algorithms for computing impulse and frequency responses
    /// and leaves methods ApplyTo() and Process() abstract.
    /// </summary>
    public abstract class LtiFilter : IFilter, IOnlineFilter
    {
        /// <summary>
        /// Transfer function
        /// </summary>
        public TransferFunction Tf { get; protected set; }

        /// <summary>
        /// The filtering algorithm that should be implemented by particular subclass
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="method">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public abstract DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto);

        /// <summary>
        /// The online filtering algorithm should be implemented by particular subclass
        /// </summary>
        /// <param name="input">Input block of samples</param>
        /// <param name="output">Block of filtered samples</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        /// <param name="method">General filtering strategy</param>
        public abstract void Process(float[] input,
                                     float[] output,
                                     int count,
                                     int inputPos = 0,
                                     int outputPos = 0,
                                     FilteringMethod method = FilteringMethod.Auto);

        /// <summary>
        /// Reset filter (clear all internal buffers)
        /// </summary>
        public abstract void Reset();

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
