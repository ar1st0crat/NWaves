using System.Collections.Generic;
using NWaves.Signals;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for direct and inverse Short-Time Fourier Transforms.
    /// </summary>
    public class Stft
    {
        /// <summary>
        /// Size of FFT (in samples)
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Overlap size (in samples)
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of the window (in samples)
        /// </summary>
        private readonly int _windowSize;
        
        /// <summary>
        /// Window type
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Pre-computed samples of the window function
        /// </summary>
        private readonly double[] _windowSamples;

        /// <summary>
        /// Constructor with necessary parameters
        /// </summary>
        /// <param name="windowSize">Size of window</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <param name="fftSize">Size of FFT</param>
        public Stft(int windowSize = 512, int hopSize = 256, WindowTypes window = WindowTypes.Rectangular, int fftSize = 512)
        {
            _fftSize = fftSize >= windowSize ? fftSize : MathUtils.NextPowerOfTwo(windowSize);
            _fft = new Fft(_fftSize);

            _hopSize = hopSize;

            _windowSize = windowSize;
            _window = window;
            _windowSamples = Window.OfType(_window, _windowSize);
        }

        /// <summary>
        /// Method for computing direct STFT of a signal block.
        /// STFT (spectrogram) is essentially the list of spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<double[]> Direct(double[] samples, int startSample, int endSample)
        {
            var block = new double[_fftSize];
            var zeroblock = new double[_fftSize - _windowSize];

            var spectrogram = new List<double[]>();

            var pos = startSample;
            for (; pos + _windowSize < endSample; pos += _hopSize)
            {
                FastCopy.ToExistingArray(samples, block, _windowSize, pos);
                FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length, 0, _windowSize);

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }

                var spectrum = new double[_fftSize / 2 + 1];
                _fft.MagnitudeSpectrum(block, spectrum);

                spectrogram.Add(spectrum);
            }

            return spectrogram;
        }
        
        /// <summary>
        /// Method for computing direct STFT of entire signal.
        /// STFT (spectrogram) is essentially the list of spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<double[]> Direct(double[] samples)
        {
            return Direct(samples, 0, samples.Length);
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">The signal under analysis</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<double[]> Direct(DiscreteSignal signal)
        {
            return Direct(signal.Samples);
        }
    }
}
