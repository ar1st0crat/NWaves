using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly float[] _windowSamples;

        /// <summary>
        /// Normalization factor
        /// </summary>
        private readonly float _norm;


        /// <summary>
        /// Constructor with necessary parameters
        /// </summary>
        /// <param name="windowSize">Size of window</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <param name="fftSize">Size of FFT</param>
        public Stft(int windowSize = 1024, int hopSize = 256, WindowTypes window = WindowTypes.Hann, int fftSize = 0)
        {
            _fftSize = fftSize >= windowSize ? fftSize : MathUtils.NextPowerOfTwo(windowSize);
            _fft = new Fft(_fftSize);

            _hopSize = hopSize;

            _windowSize = windowSize;
            _window = window;
            _windowSamples = Window.OfType(_window, _windowSize);

            // TODO: pad center!

            _norm = 2.0f / (_windowSamples.Sum(s => s * s) * _fftSize / _hopSize);

            //_norm = 2.0 * Math.Sqrt((float)_fftSize / _hopSize));
            //_norm = 2.0 / (_fftSize / 2 * (_fftSize / _hopSize));
        }

        /// <summary>
        /// Method for computing direct STFT of a signal block.
        /// STFT (spectrogram) is essentially the list of spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>STFT of the signal</returns>
        public List<Tuple<float[], float[]>> Direct(float[] samples)
        {
            var stft = new List<Tuple<float[], float[]>>();

            for (var pos = 0; pos + _windowSize < samples.Length; pos += _hopSize)
            {
                var re = new float[_fftSize];
                var im = new float[_fftSize];
                samples.FastCopyTo(re, _windowSize, pos);

                if (_window != WindowTypes.Rectangular)
                {
                    re.ApplyWindow(_windowSamples);
                }
                
                _fft.Direct(re, im);

                stft.Add(new Tuple<float[], float[]>(re, im));
            }

            return stft;
        }

        /// <summary>
        /// Inverse STFT
        /// </summary>
        /// <param name="stft"></param>
        /// <returns></returns>
        public float[] Inverse(List<Tuple<float[], float[]>> stft)
        {
            var spectraCount = stft.Count;
            var samples = new float[spectraCount * _hopSize + _windowSize];

            var re = new float[_windowSize];
            var im = new float[_windowSize];

            var pos = 0;
            for (var i = 0; i < spectraCount; i++)
            {
                stft[i].Item1.FastCopyTo(re, _windowSize);
                stft[i].Item2.FastCopyTo(im, _windowSize);

                _fft.Inverse(re, im);

                // windowing and reconstruction
                
                for (var j = 0; j < re.Length; j++)
                {
                    samples[pos + j] += re[j] * _windowSamples[j] * _norm;
                }

                pos += _hopSize;
            }

            return samples;
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">The signal under analysis</param>
        /// <returns>STFT of the signal</returns>
        public List<Tuple<float[], float[]>> Direct(DiscreteSignal signal)
        {
            return Direct(signal.Samples);
        }

        /// <summary>
        /// Method for computing a spectrogram.
        /// The spectrogram is essentially a list of power spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<float[]> Spectrogram(float[] samples)
        {
            var block = new float[_fftSize];
            var zeroblock = new float[_fftSize];

            var spectrogram = new List<float[]>();

            for (var pos = 0; pos + _windowSize < samples.Length; pos += _hopSize)
            {
                zeroblock.FastCopyTo(block, _fftSize);
                samples.FastCopyTo(block, _windowSize, pos);
                
                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }

                var spectrum = new float[_fftSize / 2 + 1];
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
        public List<float[]> Spectrogram(DiscreteSignal signal)
        {
            return Spectrogram(signal.Samples);
        }
    }
}
