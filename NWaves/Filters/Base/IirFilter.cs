using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class representing Infinite Impulse Response filters
    /// </summary>
    public class IirFilter : IFilter
    {
        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations)
        /// </summary>
        public double[] A { get; set; }

        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// </summary>
        public double[] B { get; set; }

        /// <summary>
        /// The length of truncated infinite impulse reponse
        /// </summary>
        public int ImpulseResponseLength { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="a"></param>
        /// <param name="impulseResponseLength"></param>
        public IirFilter(IEnumerable<double> b, 
                         IEnumerable<double> a,
                         int impulseResponseLength = 512)
        {
            A = a.ToArray();
            B = b.ToArray();
            ImpulseResponseLength = impulseResponseLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public virtual DiscreteSignal ApplyTo(DiscreteSignal signal,
                                              FilteringOptions filteringOptions = FilteringOptions.DifferenceEquation)
        {
            var start = Math.Max(A.Length, B.Length);

            var input = signal.Samples;
            var length = input.Length + start;
            var samples = new double[length];
            
            for (var i = start; i < length; i++)
            {
                for (var j = 0; j < B.Length; j++)
                {
                    samples[i] += B[j] * input[i - j];
                }
                for (var j = 1; j < A.Length; j++)
                {
                    samples[i] -= A[j] * input[i - j];
                }
            }

            return signal;
        }

        /// <summary>
        /// Method calculates the Frequency Response of a filter
        /// by taking FFT of truncated impulse response
        /// </summary>
        public ComplexDiscreteSignal FrequencyResponse
        {
            get
            {
                var real = ImpulseResponse.Samples;
                var imag = new double [ImpulseResponseLength];

                Transform.Fft(real, imag, ImpulseResponseLength);

                return new ComplexDiscreteSignal(ImpulseResponse.SamplingRate, real, imag);
            }
        }

        /// <summary>
        /// Method calculates the Impulse Response of a filter
        /// by 
        /// </summary>
        public DiscreteSignal ImpulseResponse
        {
            get
            {
                var impulse = new DiscreteSignal(1, ImpulseResponseLength) {[0] = 1.0};
                return ApplyTo(impulse);
            }
        }
    }
}
