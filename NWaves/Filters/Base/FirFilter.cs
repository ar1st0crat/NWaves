using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class representing Finite Impulse Response filters
    /// </summary>
    public class FirFilter : LtiFilter
    {
        /// <summary>
        /// Filter's kernel.
        /// 
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// </summary>
        public double[] Kernel { get; set; }

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public FirFilter()
        {
            ImpulseResponseLength = DefaultImpulseResponseLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="impulseResponseLength"></param>
        public FirFilter(IEnumerable<double> kernel, int impulseResponseLength = DefaultImpulseResponseLength)
        {
            Kernel = kernel.ToArray();
            ImpulseResponseLength = impulseResponseLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal, 
                                               FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            switch (filteringOptions)
            {
                case FilteringOptions.Auto:
                case FilteringOptions.OverlapAdd:
                    return signal.Copy();

                case FilteringOptions.Custom:
                case FilteringOptions.DifferenceEquation:
                    {
                        var start = Kernel.Length;

                        var input = signal.Samples;
                        var length = input.Length;
                        var samples = new double[length];

                        for (var i = start; i < length; i++)
                        {
                            for (var j = 0; j < Kernel.Length; j++)
                            {
                                samples[i] += Kernel[j] * input[i - j];
                            }
                        }

                        return new DiscreteSignal(signal.SamplingRate, samples);
                    }
                
                // Currently just return copy for any other options
                default:
                    return signal.Copy();
            }
        }

        /// <summary>
        /// Frequency response of an FIR filter is the FT of its impulse response
        /// </summary>
        public override ComplexDiscreteSignal FrequencyResponse
        {
            get
            {
                var real = new double[ImpulseResponseLength];
                var imag = new double[ImpulseResponseLength];

                Buffer.BlockCopy(Kernel, 0, real, 0, Kernel.Length * 8);

                Transform.Fft(real, imag, ImpulseResponseLength);

                return new ComplexDiscreteSignal(1, real, imag);
            }
        }

        /// <summary>
        /// Impulse response of an FIR filter is its kernel
        /// </summary>
        public override DiscreteSignal ImpulseResponse => new DiscreteSignal(1, Kernel);
    }
}
