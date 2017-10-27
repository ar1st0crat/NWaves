using System;
using NWaves.Signals;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Method implements the Overlap-Add algorithm of a block convolution
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="kernel"></param>
        /// <param name="fftSize"></param>
        /// <returns></returns>
        public static DiscreteSignal OverlapAdd(DiscreteSignal signal, DiscreteSignal kernel, int fftSize)
        {
            var m = kernel.Samples.Length;

            if (m > fftSize)
            {
                throw new ArgumentException("Kernel length must not exceed the size of FFT!");
            }
            
            var filtered = new DiscreteSignal(signal.SamplingRate, signal.Samples.Length);

            var hopSize = fftSize - m + 1;
            var i = 0;
            while (i + fftSize < signal.Samples.Length)
            {
                var res = Convolve(signal[i, i + hopSize], kernel);

                for (var j = 0; j < m - 1; j++)
                {
                    filtered[i + j] += res[j];
                }

                for (var j = m - 1; j < res.Samples.Length; j++)
                {
                    filtered[i + j] = res[j];
                }

                i += hopSize;
            }

            return filtered;
        }

        /// <summary>
        /// Method implements the Overlap-Save algorithm of a block convolution
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="kernel"></param>
        /// <param name="fftSize"></param>
        /// <returns></returns>
        public static DiscreteSignal OverlapSave(DiscreteSignal signal, DiscreteSignal kernel, int fftSize)
        {
            var m = kernel.Samples.Length;

            if (m > fftSize)
            {
                throw new ArgumentException("Kernel length must not exceed the size of FFT!");
            }

            var filtered = new DiscreteSignal(signal.SamplingRate, signal.Samples.Length);

            var hopSize = fftSize - m + 1;
            var i = m - 1;
            while (i + fftSize < signal.Samples.Length - m + 1)
            {
                var res = Convolve(signal[i, i + fftSize], kernel);

                for (var j = m - 1; j < res.Samples.Length; j++)
                {
                    filtered[i + j] = res.Samples[j];
                }

                i += hopSize;
            }

            return filtered;
        }
    }
}
