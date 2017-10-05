using System;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Fast convolution via FFT
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

            var real1 = new double[fftSize];
            signal1.Samples.CopyTo(real1, 0);
            var imag1 = new double[fftSize];

            var real2 = new double[fftSize];
            signal2.Samples.CopyTo(real2, 0);
            var imag2 = new double[fftSize];

            Transform.Fft(real1, imag1, fftSize);
            Transform.Fft(real2, imag2, fftSize);

            // TODO: refactor ComplexMultiply()

            var real = new double[fftSize];
            var imag = new double[fftSize];

            for (var i = 0; i < fftSize; i++)
            {
                real[i] = (real1[i] * real2[i] - imag1[i] * imag2[i]) / fftSize;
                imag[i] = (real1[i] * imag2[i] + imag1[i] * real2[i]) / fftSize;
            }

            Transform.Ifft(real, imag, fftSize);

            return new DiscreteSignal(signal1.SamplingRate, real);
        }

        /// <summary>
        /// Direct convolution by formula in time domain
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal ConvolveDirect(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            return Convolve(signal1, signal2);
        }

        /// <summary>
        /// Fast correlation via FFT
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var reversedKernel = new DiscreteSignal(signal2.SamplingRate, signal2.Samples.Reverse());

            return Convolve(signal1, reversedKernel);
        }

        /// <summary>
        /// Direct cross-correlation by formula in time domain
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
