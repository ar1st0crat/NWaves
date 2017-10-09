using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class representing Infinite Impulse Response filters
    /// </summary>
    public class IirFilter : FilterBase
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
        /// Parameterless constructor
        /// </summary>
        public IirFilter()
        {
            ImpulseResponseLength = DefaultImpulseResponseLength;
        }

        /// <summary>
        /// Parameterized constructor
        /// </summary>
        /// <param name="b">TF numerator coefficients</param>
        /// <param name="a">TF denominator coefficients</param>
        /// <param name="impulseResponseLength">Length of truncated impulse response</param>
        public IirFilter(IEnumerable<double> b, 
                         IEnumerable<double> a,
                         int impulseResponseLength = DefaultImpulseResponseLength)
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
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringOptions filteringOptions = FilteringOptions.DifferenceEquation)
        {
            switch (filteringOptions)
            {
                case FilteringOptions.Custom:
                case FilteringOptions.DifferenceEquation:
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

                    return new DiscreteSignal(signal.SamplingRate, samples);
                }
                // Currently just return copy for any other options
                default:
                    return signal.Copy();
            }
        }
    }
}
