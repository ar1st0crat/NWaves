using System;
using NWaves.Filters.Base;
using NWaves.Operations.BlockConvolution;
using NWaves.Signals;

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
                                                   FilteringMethod method = FilteringMethod.OverlapAdd)
        {
            //fftSize = 512;
            if (kernel.Length > fftSize)
            {
                throw new ArgumentException("Kernel length must not exceed the size of FFT!");
            }

            if (signal.Length < fftSize)
            {
                return signal.Copy();
            }

            var blockConvolver = new BlockConvolver(kernel.Samples, fftSize);
            var filtered = new float[signal.Length + kernel.Length - 1];

            var chunkSize = blockConvolver.ChunkSize;

            for (var i = 0; i < signal.Length; i += chunkSize)
            {
                blockConvolver.Process(signal.Samples, filtered, chunkSize, i, i, method);
            }

            return new DiscreteSignal(signal.SamplingRate, filtered);
        }
    }
}
