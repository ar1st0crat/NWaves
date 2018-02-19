using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Fast deconvolution via FFT for general complex-valued case.
        /// 
        /// 
        /// NOTE!
        /// 
        /// Deconvolution is an experimental feature.
        /// It's problematic due to division by zero.
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal Deconvolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel)
        {
            var length = signal.Length - kernel.Length + 1;

            var fftSize = MathUtils.NextPowerOfTwo(signal.Length);
            var fft = new Fft(fftSize);

            signal = signal.ZeroPadded(fftSize);
            kernel = kernel.ZeroPadded(fftSize);

            // 1) do FFT of both signals

            fft.Direct(signal.Real, signal.Imag);
            fft.Direct(kernel.Real, kernel.Imag);

            for (var i = 0; i < fftSize; i++)
            {
                signal.Real[i] += 1e-10;
                signal.Imag[i] += 1e-10;
                kernel.Real[i] += 1e-10;
                kernel.Imag[i] += 1e-10;
            }

            // 2) do complex division of spectra

            var spectrum = signal.Divide(kernel);

            // 3) do inverse FFT of resulting spectrum

            fft.Inverse(spectrum.Real, spectrum.Imag);

            // 4) return resulting meaningful part of the signal (truncate to N - M + 1)

            return new ComplexDiscreteSignal(signal.SamplingRate,
                                FastCopy.ArrayFragment(spectrum.Real, length),
                                FastCopy.ArrayFragment(spectrum.Imag, length));
        }
    }
}
