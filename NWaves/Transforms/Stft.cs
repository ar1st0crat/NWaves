using System.Collections.Generic;
using NWaves.Signals;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Transforms
{
    public static partial class Transform
    {
        /// <summary>
        /// STFT (spectrogram) is essentially the list of spectra in time
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <param name="windowSize">Size of window</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <returns>Spectrogram of the signal</returns>
        public static List<double[]> Stft(double[] samples, int windowSize = 512, int hopSize = 256, WindowTypes window = WindowTypes.Rectangular, int fftSize = 512)
        {
            fftSize = fftSize >= windowSize ? fftSize : MathUtils.NextPowerOfTwo(windowSize);

            var block = new double [fftSize];
            var zeroblock = new double [fftSize - windowSize];

            var windowSamples = Window.OfType(window, windowSize);

            var spectrogram = new List<double[]>();

            var pos = 0;
            for (; pos + windowSize < samples.Length; pos += hopSize)
            {
                FastCopy.ToExistingArray(samples, block, windowSize, pos);
                FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length, 0, windowSize);

                if (window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(windowSamples);
                }

                spectrogram.Add(MagnitudeSpectrum(block, fftSize));
            }

            return spectrogram;
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">The signal under analysis</param>
        /// <param name="windowSize">Size of window</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <returns>Spectrogram of the signal</returns>
        public static List<double[]> Stft(DiscreteSignal signal, int windowSize = 512, int hopSize = 256, WindowTypes window = WindowTypes.Rectangular, int fftSize = 512)
        {
            return Stft(signal.Samples, windowSize, hopSize, window, fftSize);
        }
    }
}
