﻿using System.Linq;
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

            var real1 = new double[fftSize];
            var imag1 = new double[fftSize];
            var real2 = new double[fftSize];
            var imag2 = new double[fftSize];

            FastCopy.ToExistingArray(signal1.Samples, real1, signal1.Length);
            FastCopy.ToExistingArray(signal2.Samples, real2, signal2.Length);

            // 1) do FFT of both signals

            Fft.Direct(real1, imag1, fftSize);
            Fft.Direct(real2, imag2, fftSize);

            // 2) do complex multiplication of spectra and normalize

            for (var i = 0; i < fftSize; i++)
            {
                var re = real1[i] * real2[i] - imag1[i] * imag2[i];
                var im = real1[i] * imag2[i] + imag1[i] * real2[i];
                real1[i] = re / fftSize;
                imag1[i] = im / fftSize;
            }

            // 3) do inverse FFT of resulting spectrum

            Fft.Inverse(real1, imag1, fftSize);

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

            signal1 = signal1.ZeroPadded(fftSize);
            signal2 = signal2.ZeroPadded(fftSize);

            // 1) do FFT of both signals

            Fft.Direct(signal1.Real, signal1.Imag, fftSize);
            Fft.Direct(signal2.Real, signal2.Imag, fftSize);

            // 2) do complex multiplication of spectra

            var spectrum = signal1.Multiply(signal2);
            
            // 3) do inverse FFT of resulting spectrum

            Fft.Inverse(spectrum.Real, spectrum.Imag, fftSize);

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
        /// <param name="center">Position of central sample for the case of 2*M-1 convolution (if it is set then resulting array has length of M)</param>
        public static void Convolve(double[] real1, double[] imag1, double[] real2, double[] imag2, double[] res, int center = 0)
        {
            var fftSize = real1.Length;
            
            // 1) do FFT of both signals

            Fft.Direct(real1, imag1, fftSize);
            Fft.Direct(real2, imag2, fftSize);

            // 2) do complex multiplication of spectra and normalize

            for (var i = 0; i < fftSize; i++)
            {
                var re = real1[i] * real2[i] - imag1[i] * imag2[i];
                var im = real1[i] * imag2[i] + imag1[i] * real2[i];
                real1[i] = re / fftSize;
                imag1[i] = im / fftSize;
            }

            // 3) do inverse FFT of resulting spectrum

            Fft.Inverse(real1, imag1, fftSize);

            // 4) return output array

            if (center > 0)
            {
                FastCopy.ToExistingArray(real1, res, center, center - 1);
            }
            else
            {
                FastCopy.ToExistingArray(real1, res, fftSize);
            }
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
        public static void CrossCorrelate(double[] real1, double[] imag1, double[] real2, double[] imag2, double[] res, int center = 0)
        {
            // reverse second signal
            for (var i = 0; i < center; i++)
            {
                real2[i] = real1[center - 1 - i];
                imag2[i] = imag1[center - 1 - i];
            }

            Convolve(real1, imag1, real2, imag2, res, center);
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
