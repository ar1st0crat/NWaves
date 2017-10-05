using System;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal Convolve(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            if (signal1.SamplingRate != signal2.SamplingRate)
            {
                throw new ArgumentException("Sampling rates should be the same!");
            }

            var fftSize = signal1.Samples.Length + signal2.Samples.Length - 1;

            var real1 = signal1.Samples;
            var imag1 = new double[fftSize];
            var real2 = signal2.Samples;
            var imag2 = new double[fftSize];

            Transform.Fft(real1, imag1, fftSize);
            Transform.Fft(real2, imag2, fftSize);

            //refactor ComplexMultiply()

            var real = new double[fftSize];
            var imag = new double[fftSize];

            for (var i = 0; i < fftSize; i++)
            {
                real[i] = real1[i] * real2[i] - imag1[i] * imag2[i];
                imag[i] = real1[i] * imag2[i] - imag1[i] * real2[i];
            }

            Transform.Ifft(real, imag, fftSize);

            return new DiscreteSignal(real, signal1.SamplingRate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal ConvolveDirect(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            return Convolve(signal1, signal2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var reversedKernel = new DiscreteSignal(signal2.Samples.Reverse(), signal2.SamplingRate);

            return Convolve(signal1, reversedKernel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal CrossCorrelateDirect(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            return CrossCorrelate(signal1, signal2);
        }
    }
}
