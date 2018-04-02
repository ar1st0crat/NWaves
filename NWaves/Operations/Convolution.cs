using System;
using System.Linq;
using System.Numerics;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Fast convolution via FFT of real-valued signals.
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal Convolve(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            // prepare blocks in memory:

            var length = signal1.Length + signal2.Length - 1;

            var fftSize = MathUtils.NextPowerOfTwo(length);
            var fft = new Fft(fftSize);

            var real1 = new float[fftSize];
            var imag1 = new float[fftSize];
            var real2 = new float[fftSize];
            var imag2 = new float[fftSize];

            signal1.Samples.FastCopyTo(real1, signal1.Length);
            signal2.Samples.FastCopyTo(real2, signal2.Length);

            // 1) do FFT of both signals

            fft.Direct(real1, imag1);
            fft.Direct(real2, imag2);

            // 2) do complex multiplication of spectra and normalize

            for (var i = 0; i < fftSize; i++)
            {
                var re = real1[i] * real2[i] - imag1[i] * imag2[i];
                var im = real1[i] * imag2[i] + imag1[i] * real2[i];
                real1[i] = re / fftSize;
                imag1[i] = im / fftSize;
            }

            // 3) do inverse FFT of resulting spectrum

            fft.Inverse(real1, imag1);

            // 4) return resulting meaningful part of the signal (truncate size to N + M - 1)

            return new DiscreteSignal(signal1.SamplingRate, real1).First(length);
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
            var fft = new Fft64(fftSize);

            signal1 = signal1.ZeroPadded(fftSize);
            signal2 = signal2.ZeroPadded(fftSize);

            // 1) do FFT of both signals

            fft.Direct(signal1.Real, signal1.Imag);
            fft.Direct(signal2.Real, signal2.Imag);

            // 2) do complex multiplication of spectra

            var spectrum = signal1.Multiply(signal2);
            
            // 3) do inverse FFT of resulting spectrum

            fft.Inverse(spectrum.Real, spectrum.Imag);

            // 3a) normalize

            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum.Real[i] /= fftSize;
                spectrum.Imag[i] /= fftSize;
            }

            // 4) return resulting meaningful part of the signal (truncate size to N + M - 1)

            return new ComplexDiscreteSignal(signal1.SamplingRate, spectrum.Real, spectrum.Imag).First(length);
        }

        /// <summary>
        /// Fast convolution via FFT for arrays of samples (maximally in-place).
        /// This version is best suited for block processing when memory needs to be reused.
        /// Input arrays must have size equal to the size of FFT.
        /// </summary>
        /// <param name="real1">Real parts of the 1st signal (zero-padded)</param>
        /// <param name="imag1">Imaginary parts of the 1st signal (zero-padded)</param>
        /// <param name="real2">Real parts of the 2nd signal (zero-padded)</param>
        /// <param name="imag2">Imaginary parts of the 2nd signal (zero-padded)</param>
        /// <param name="res">Real parts of resulting convolution (zero-padded if center == 0)</param>
        /// <param name="center">
        /// Position of central sample for the case of 2*M-1 convolution 
        /// (if it is set then resulting array has length of M)
        /// </param>
        public static void Convolve(float[] real1, float[] imag1, float[] real2, float[] imag2, float[] res, int center = 0)
        {
            var fftSize = real1.Length;
            var fft = new Fft(fftSize);

            // 1) do FFT of both signals

            fft.Direct(real1, imag1);
            fft.Direct(real2, imag2);

            // 2) do complex multiplication of spectra and normalize

            for (var i = 0; i < fftSize; i++)
            {
                var re = real1[i] * real2[i] - imag1[i] * imag2[i];
                var im = real1[i] * imag2[i] + imag1[i] * real2[i];
                real1[i] = re / fftSize;
                imag1[i] = im / fftSize;
            }

            // 3) do inverse FFT of resulting spectrum

            fft.Inverse(real1, imag1);

            // 4) return output array

            if (center > 0)
            {
                real1.FastCopyTo(res, center, center - 1);
            }
            else
            {
                real1.FastCopyTo(res, fftSize);
            }
        }

        /// <summary>
        /// Fast convolution for double arrays
        /// </summary>
        /// <param name="samples1"></param>
        /// <param name="samples2"></param>
        /// <returns></returns>
        public static double[] Convolve(double[] samples1, double[] samples2)
        {
            return Convolve(new ComplexDiscreteSignal(1, samples1), 
                            new ComplexDiscreteSignal(1, samples2)).Real;
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
        /// Fast cross-correlation via FFT
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal CrossCorrelate(ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            var reversedKernel = 
                new ComplexDiscreteSignal(signal2.SamplingRate, signal2.Real.Reverse(), signal2.Imag.Reverse());

            return Convolve(signal1, reversedKernel);
        }

        /// <summary>
        /// Fast cross-correlation via FFT for arrays of samples (maximally in-place).
        /// This version is best suited for block processing when memory needs to be reused.
        /// Input arrays must have size equal to the size of FFT.
        /// </summary>
        /// <param name="real1">Real parts of the 1st signal (zero-padded)</param>
        /// <param name="imag1">Imaginary parts of the 1st signal (zero-padded)</param>
        /// <param name="real2">Real parts of the 2nd signal (zero-padded)</param>
        /// <param name="imag2">Imaginary parts of the 2nd signal (zero-padded)</param>
        /// <param name="res">Real parts of resulting cross-correlation (zero-padded if center == 0)</param>
        /// <param name="center">Position of central sample for the case of 2*CENTER-1 cross-correlation 
        /// (if it is set then resulting array has length of CENTER)</param>
        public static void CrossCorrelate(float[] real1, float[] imag1, float[] real2, float[] imag2, float[] res, int center = 0)
        {
            // reverse second signal
            for (var i = 0; i < center; i++)
            {
                real2[i] = real1[center - 1 - i];
                imag2[i] = imag1[center - 1 - i];
            }

            Convolve(real1, imag1, real2, imag2, res, center);
        }

        /// <summary>
        /// Fast deconvolution via FFT for general complex-valued case.
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
            // first, try to divide polynomials

            var div = MathUtils.PolynomialDivision(signal.Real.Zip(signal.Imag, (r, i) => new Complex(r, i)).ToArray(),
                                                   kernel.Real.Zip(kernel.Imag, (r, i) => new Complex(r, i)).ToArray());

            var quotient = div[0];
            var remainder = div[1];
            if (remainder.All(d => Math.Abs(d.Real)      < 1e-10) && 
                remainder.All(d => Math.Abs(d.Imaginary) < 1e-10))
            {
                return new ComplexDiscreteSignal(signal.SamplingRate, 
                                                 quotient.Select(q => q.Real),
                                                 quotient.Select(q => q.Imaginary));
            }

            // ... deconvolve via FFT

            var length = signal.Length - kernel.Length + 1;

            var fftSize = MathUtils.NextPowerOfTwo(signal.Length);
            var fft = new Fft64(fftSize);

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
                                             spectrum.Real.FastCopyFragment(length),
                                             spectrum.Imag.FastCopyFragment(length));
        }


        /****************************************************************************
         * 
         *    The following methods are included mainly for educational purposes
         * 
         ***************************************************************************/

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

            var conv = new float[length];

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

            var corr = new float[length];

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
