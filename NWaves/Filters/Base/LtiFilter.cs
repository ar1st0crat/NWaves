using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Base class for all kinds of LTI filters.
    /// Provides general algorithms for computing impulse and frequency responses
    /// and leaves methods ApplyTo() and Process() abstract.
    /// </summary>
    public abstract class LtiFilter : IFilter
    {
        /// <summary>
        /// Transfer function
        /// </summary>
        public TransferFunction Tf { get; protected set; }

        /// <summary>
        /// The filtering algorithm that should be implemented by particular subclass
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="filteringOptions">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public abstract DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringOptions filteringOptions = FilteringOptions.Auto);

        /// <summary>
        /// The online filtering algorithm should be implemented by particular subclass
        /// </summary>
        /// <param name="input">Input block of samples</param>
        /// <param name="filteringOptions">General filtering strategy</param>
        /// <returns>Filtered block</returns>
        public abstract float[] Process(float[] input,
                                        FilteringOptions filteringOptions = FilteringOptions.Auto);

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

        /// <summary>
        /// NOTE. For educational purposes and for testing online filtering.
        /// 
        /// Implementation of offline filtering in time domain frame-by-frame.
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="frameSize"></param>
        /// <returns></returns>        
        public DiscreteSignal ApplyFilterCircularBuffer(DiscreteSignal signal, int frameSize = 4096)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            var i = 0;
            while (i + frameSize < input.Length)
            {
                var buf = input.FastCopyFragment(frameSize, i);
                var filtered = Process(buf);
                filtered.FastCopyTo(output, frameSize, 0, i);
                i += frameSize;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
