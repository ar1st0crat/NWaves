using System.Linq;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Fast convolution via FFT for real-valued signals
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal Convolve(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var length = signal1.Length + signal2.Length - 1;

            var fftSize = MathUtils.NextPowerOfTwo(length);

            var real1 = new double[fftSize];
            var imag1 = new double[fftSize];
            var real2 = new double[fftSize];
            var imag2 = new double[fftSize];

            FastCopy.ToExistingArray(signal1.Samples, real1, signal1.Length);
            FastCopy.ToExistingArray(signal2.Samples, real2, signal2.Length);

            // 1) do FFT of both signals

            Transform.Fft(real1, imag1, fftSize);
            Transform.Fft(real2, imag2, fftSize);

            // 2) do complex multiplication of spectra

            var s1 = new ComplexDiscreteSignal(1, real1, imag1);    // memory-costless wrap into complex signal
            var s2 = new ComplexDiscreteSignal(1, real2, imag2);    // memory-costless wrap into complex signal
            var spectrum = s1.Multiply(s2);

            // 3) do inverse FFT of resulting spectrum

            Transform.Ifft(spectrum.Real, spectrum.Imag, fftSize);

            // 3a) normalize

            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum.Real[i] /= fftSize;
            }

            // 4) return resulting meaningful part of the signal (truncate size to N + M - 1)

            return new DiscreteSignal(signal1.SamplingRate, FastCopy.ArrayFragment(spectrum.Real, length));
        }

        /// <summary>
        /// Fast convolution via FFT for general complex-valued case
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal Convolve(ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            var length = signal1.Length + signal2.Length - 1;

            var fftSize = MathUtils.NextPowerOfTwo(length);

            signal1 = signal1.ZeroPadded(fftSize);
            signal2 = signal2.ZeroPadded(fftSize);

            // 1) do FFT of both signals

            Transform.Fft(signal1.Real, signal1.Imag, fftSize);
            Transform.Fft(signal2.Real, signal2.Imag, fftSize);

            // 2) do complex multiplication of spectra

            var spectrum = signal1.Multiply(signal2);
            
            // 3) do inverse FFT of resulting spectrum

            Transform.Ifft(spectrum.Real, spectrum.Imag, fftSize);

            // 3a) normalize

            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum.Real[i] /= fftSize;
                spectrum.Imag[i] /= fftSize;
            }

            // 4) return resulting meaningful part of the signal (truncate size to N + M - 1)

            return new ComplexDiscreteSignal(signal1.SamplingRate, 
                                FastCopy.ArrayFragment(spectrum.Real, length),
                                FastCopy.ArrayFragment(spectrum.Imag, length));
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
                        conv[n] += a[n - k] * b[k];
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
