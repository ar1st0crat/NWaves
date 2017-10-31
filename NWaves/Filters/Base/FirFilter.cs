﻿using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

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
                case FilteringOptions.DifferenceEquation:
                {
                    return ApplyFilterDirectly(signal);
                }
                case FilteringOptions.OverlapAdd:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * Kernel.Length);
                    return Operation.OverlapAdd(signal, new DiscreteSignal(signal.SamplingRate, Kernel), fftSize);
                }
                case FilteringOptions.OverlapSave:
                {
                    var fftSize = MathUtils.NextPowerOfTwo(4 * Kernel.Length);
                    return Operation.OverlapSave(signal, new DiscreteSignal(signal.SamplingRate, Kernel), fftSize);
                }
                default:
                {
                    return ApplyFilterCircularBuffer(signal);
                }
            }
        }

        /// <summary>
        /// The most straightforward implementation of the difference equation:
        /// code the difference equation as it is
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyFilterDirectly(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var kernel = Kernel;

            var samples = new double[input.Length];

            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < kernel.Length; k++)
                {
                    if (n >= k) samples[n] += kernel[k] * input[n - k];
                }
            }

            return new DiscreteSignal(signal.SamplingRate, samples);
        }

        /// <summary>
        /// More efficient implementation of filtering in time domain:
        /// use circular buffers for recursive and non-recursive delay lines.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>        
        public DiscreteSignal ApplyFilterCircularBuffer(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var kernel = Kernel;

            var samples = new double[input.Length];

            // buffers for delay lines:
            var wb = new double[kernel.Length];
            
            var wbpos = wb.Length - 1;
            
            for (var n = 0; n < input.Length; n++)
            {
                wb[wbpos] = input[n];

                var pos = 0;
                for (var k = wbpos; k < kernel.Length; k++)
                {
                    samples[n] += kernel[pos++] * wb[k];
                }
                for (var k = 0; k < wbpos; k++)
                {
                    samples[n] += kernel[pos++] * wb[k];
                }

                wbpos--;
                if (wbpos < 0) wbpos = wb.Length - 1;
            }

            return new DiscreteSignal(signal.SamplingRate, samples);
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

        /// <summary>
        /// Zeros of the transfer function
        /// </summary>
        public override ComplexDiscreteSignal Zeros
        {
            get
            {
                if (Kernel.Length <= 1)
                {
                    return null;
                }

                var roots = MathUtils.PolynomialRoots(Kernel.Reverse().ToArray(), new double[Kernel.Length]);

                return new ComplexDiscreteSignal(1, roots.Item1, roots.Item2);
            }
        }

        /// <summary>
        /// Poles of the transfer function (FIR filter does not have poles)
        /// </summary>
        public override ComplexDiscreteSignal Poles => null;
    }
}
