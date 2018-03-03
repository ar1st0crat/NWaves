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
        /// <returns>STFT of the signal</returns>
        public List<ComplexDiscreteSignal> Direct(double[] samples)
        {
            var stft = new List<ComplexDiscreteSignal>();

            for (var pos = 0; pos + _windowSize < samples.Length; pos += _hopSize)
            {
                var re = new double[_fftSize];
                var im = new double[_fftSize];
                FastCopy.ToExistingArray(samples, re, _windowSize, pos);

                if (_window != WindowTypes.Rectangular)
                {
                    re.ApplyWindow(_windowSamples);
                }
                
                _fft.Direct(re, im);

                stft.Add(new ComplexDiscreteSignal(1, re, im));
            }

            return stft;
        }

        /// <summary>
        /// Inverse STFT
        /// </summary>
        /// <param name="stft"></param>
        /// <returns></returns>
        public double[] Inverse(List<ComplexDiscreteSignal> stft)
        {
            var spectraCount = stft.Count;
            var samples = new double[spectraCount * _hopSize];

            var pos = 0;
            for (var i = 0; i < spectraCount; i++)
            {
                var re = FastCopy.EntireArray(stft[i].Real);
                var im = FastCopy.EntireArray(stft[i].Imag);
                
                if (_window != WindowTypes.Rectangular)
                {
                    re.ApplyWindow(_windowSamples);
                }

                _fft.Inverse(re, im);

                for (var j = 0; j < re.Length; j++)
                {
                    samples[pos + j] += re[j];
                }

                pos += _windowSize;
            }

            return samples;
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">The signal under analysis</param>
        /// <returns>STFT of the signal</returns>
        public List<ComplexDiscreteSignal> Direct(DiscreteSignal signal)
        {
            return Direct(signal.Samples);
        }

        /// <summary>
        /// Method for computing a spectrogram.
        /// The spectrogram is essentially a list of power spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<double[]> Spectrogram(double[] samples)
        {
            var block = new double[_fftSize];
            var zeroblock = new double[_fftSize];

            var spectrogram = new List<double[]>();

            for (var pos = 0; pos + _windowSize < samples.Length; pos += _hopSize)
            {
                FastCopy.ToExistingArray(zeroblock, block, _fftSize);
                FastCopy.ToExistingArray(samples, block, _windowSize, pos);
                
                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }

                var spectrum = new double[_fftSize / 2 + 1];
                _fft.PowerSpectrum(block, spectrum);

                spectrogram.Add(spectrum);
            }

            return spectrogram;
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<double[]> Spectrogram(DiscreteSignal signal)
        {
            return Spectrogram(signal.Samples);
        }
    }
}
