using System;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class representing Finite Impulse Response filters
    /// </summary>
    public class FirFilter : IFilter
    {
        /// <summary>
        /// Filter's kernel.
        /// 
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// </summary>
        public double[] Kernel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public virtual DiscreteSignal ApplyTo(DiscreteSignal signal, 
                                              FilteringOptions filteringOptions = FilteringOptions.OverlapAdd)
        {
            return signal;
        }

        /// <summary>
        /// 
        /// </summary>
        public ComplexDiscreteSignal FrequencyResponse
        {
            get
            {
                var real = new double[512];
                var imag = new double[512];

                Buffer.BlockCopy(Kernel, 0, real, 0, 512 * 8);

                Transform.Fft(real, imag, 512);

                return new ComplexDiscreteSignal(ImpulseResponse.SamplingRate, real, imag);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DiscreteSignal ImpulseResponse => new DiscreteSignal(1, Kernel);
    }
}
