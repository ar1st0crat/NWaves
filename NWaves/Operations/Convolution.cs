using System;
using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

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
            var length = signal1.Samples.Length + signal2.Samples.Length - 1;

            var fftSize = MathUtils.NextPowerOfTwo(length);

            var complex1 = signal1.ToComplex(fftSize);
            var complex2 = signal2.ToComplex(fftSize);
            
            // 1) do FFT of both signals

            Transform.Fft(complex1.Real, complex1.Imag, fftSize);
            Transform.Fft(complex2.Real, complex2.Imag, fftSize);

            // 2) do complex multiplication of spectra

            var spectrum = complex1.Multiply(complex2);
            
            // 3) do inverse FFT of resulting spectrum

            Transform.Ifft(spectrum.Real, spectrum.Imag, fftSize);

            // 4) return resulting real-valued part of the signal (truncate size to N + M - 1)

            return new DiscreteSignal(signal1.SamplingRate, FastCopy.ArrayFragment(spectrum.Real, length));
        }

        /// <summary>
        /// Direct convolution by formula in time domain
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal ConvolveDirect(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var a = signal1.Samples;
            var b = signal2.Samples;
            var length = a.Length + b.Length - 1;
            
            var conv = new double[length];
            
            for (var n = 0; n < length; n++)
            {
                for (var k = 0; k < b.Length; k++)
                {
                    if (n >= k && n - k < a.Length)
                    {
                        conv[n] += a[n - k]*b[k];
                    }
                }
            }

            return new DiscreteSignal(signal1.SamplingRate, conv);
        }

        /// <summary>
        /// Fast cross-correlation via FFT
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
            var a = signal1.Samples;
            var b = signal2.Samples;
            var length = a.Length + b.Length - 1;

            var corr = new double[length];

            for (var n = 0; n < length; n++)
            {
                var pos = b.Length - 1;
                for (var k = 0; k < b.Length; k++)
                {
                    if (n >= k && n - k < a.Length)
                    {
                        corr[n] += a[n - k] * b[pos];
                    }
                    pos--;
                }
            }

            return new DiscreteSignal(signal1.SamplingRate, corr);
        }
    }
}
