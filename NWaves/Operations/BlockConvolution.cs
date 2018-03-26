using System;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Method implements block convolution of signals (using either OLA or OLS algorithm)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="kernel"></param>
        /// <param name="fftSize"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static DiscreteSignal BlockConvolve(DiscreteSignal signal,
                                                   DiscreteSignal kernel,
                                                   int fftSize,
                                                   BlockConvolution method = BlockConvolution.OverlapAdd)
        {
            var m = kernel.Length;

            if (m > fftSize)
            {
                throw new ArgumentException("Kernel length must not exceed the size of FFT!");
            }

            var fft = new Fft(fftSize);

            // pre-compute kernel's FFT:

            var kernelReal = kernel.Samples.PadZeros(fftSize);
            var kernelImag = new float[fftSize];

            fft.Direct(kernelReal, kernelImag);

            // reserve space for current signal block:

            var blockReal = new float[fftSize];
            var blockImag = new float[fftSize];
            var zeroblock = new float[fftSize];

            // reserve space for resulting spectrum at each step:

            var spectrumReal = new float[fftSize];
            var spectrumImag = new float[fftSize];

            var filtered = new DiscreteSignal(signal.SamplingRate, signal.Length);

            var hopSize = fftSize - m + 1;

            if (method == BlockConvolution.OverlapAdd)
            {
                var i = 0;
                while (i < signal.Length)
                {
                    // ============================== FFT CONVOLUTION SECTION =================================

                    // for better performance we inline FFT convolution here;
                    // alternatively, we could simply write:
                    //
                    //        var res = Convolve(signal[i, i + hopSize], kernel);
                    //
                    // ...but that would require unnecessary memory allocations 
                    //    and recalculating of kernel FFT at each step.

                    zeroblock.FastCopyTo(blockReal, fftSize);
                    zeroblock.FastCopyTo(blockImag, fftSize);
                    signal.Samples.FastCopyTo(blockReal, Math.Min(hopSize, signal.Length - i), i);

                    // 1) do FFT of a signal block:

                    fft.Direct(blockReal, blockImag);

                    // 2) do complex multiplication of spectra

                    for (var j = 0; j < fftSize; j++)
                    {
                        spectrumReal[j] = (blockReal[j] * kernelReal[j] - blockImag[j] * kernelImag[j]) / fftSize;
                        spectrumImag[j] = (blockReal[j] * kernelImag[j] + blockImag[j] * kernelReal[j]) / fftSize;
                    }

                    // 3) do inverse FFT of resulting spectrum

                    fft.Inverse(spectrumReal, spectrumImag);

                    // ========================================================================================

                    for (var j = 0; j < m - 1 && i + j < filtered.Length; j++)
                    {
                        filtered[i + j] += spectrumReal[j];
                    }

                    for (var j = m - 1; j < spectrumReal.Length && i + j < filtered.Length; j++)
                    {
                        filtered[i + j] = spectrumReal[j];
                    }

                    i += hopSize;
                }

                return filtered;
            }
            else
            {
                signal = new DiscreteSignal(signal.SamplingRate, m - 1).Concatenate(signal);

                var i = 0;
                while (i < signal.Length)
                {
                    // ============================== FFT CONVOLUTION SECTION =================================

                    signal.Samples.FastCopyTo(blockReal, Math.Min(fftSize, signal.Length - i), i);
                    zeroblock.FastCopyTo(blockImag, fftSize);

                    // 1) do FFT of a signal block:

                    fft.Direct(blockReal, blockImag);

                    // 2) do complex multiplication of spectra

                    for (var j = 0; j < fftSize; j++)
                    {
                        spectrumReal[j] = (blockReal[j] * kernelReal[j] - blockImag[j] * kernelImag[j]) / fftSize;
                        spectrumImag[j] = (blockReal[j] * kernelImag[j] + blockImag[j] * kernelReal[j]) / fftSize;
                    }

                    // 3) do inverse FFT of resulting spectrum

                    fft.Inverse(spectrumReal, spectrumImag);

                    // ========================================================================================


                    for (var j = 0; j + m - 1 < spectrumReal.Length && i + j < filtered.Length; j++)
                    {
                        filtered[i + j] = spectrumReal[j + m - 1];
                    }

                    i += hopSize;
                }

                return filtered;
            }
        }
    }

    /// <summary>
    /// Block convolution algorithms (methods)
    /// </summary>
    public enum BlockConvolution
    {
        OverlapAdd,
        OverlapSave
    }
}
