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
        public int Size => _fftSize;
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
        /// ISTFT normalization gain
        /// </summary>
        private readonly float _gain;

        /// <summary>
        /// Internal buffers
        /// </summary>
        private float[] _re;
        private float[] _im;
        private float[] _zeroblock;


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

            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _zeroblock = new float[_fftSize];

            _gain = (float)(Math.Sqrt(2))/2 / (_fftSize * _windowSamples.Select(w => w * w).Sum() / _hopSize);
        }

        /// <summary>
        /// Method for computing direct STFT of a signal block.
        /// STFT (spectrogram) is essentially the list of spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>STFT of the signal</returns>
        public List<Tuple<float[], float[]>> Direct(float[] samples)
        {
            // pre-allocate memory:

            var len = (samples.Length - _windowSize) / _hopSize;

            var stft = new List<Tuple<float[], float[]>>();
            for (var i = 0; i <= len; i++)
            {
                stft.Add(new Tuple<float[], float[]>(new float[_fftSize], new float[_fftSize]));
            }

            // stft:

            for (int pos = 0, i = 0; pos + _windowSize < samples.Length; pos += _hopSize, i++)
            {
                samples.FastCopyTo(stft[i].Item1, _windowSize, pos);

                if (_window != WindowTypes.Rectangular)
                {
                    _re.ApplyWindow(_windowSamples);
                }
                
                _fft.Direct(stft[i].Item1, stft[i].Item2);
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
            var output = new float[spectraCount * _hopSize + _fftSize];

            var pos = 0;
            for (var i = 0; i < spectraCount; i++)
            {
                stft[i].Item1.FastCopyTo(_re, _fftSize);
                stft[i].Item2.FastCopyTo(_im, _fftSize);

                _fft.Inverse(_re, _im);

                // windowing and reconstruction
                
                for (var j = 0; j < _re.Length; j++)
                {
                    output[pos + j] += _re[j] * _windowSamples[j];
                }

                for (var j = 0; j < _hopSize; j++)
                {
                    output[pos + j] *= _gain;
                }

                pos += _hopSize;
            }

            for (var j = 0; j < _windowSize; j++)
            {
                output[pos + j] *= _gain;
            }

            return output;
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
            // pre-allocate memory:

            var len = (samples.Length - _windowSize) / _hopSize;

            var spectrogram = new List<float[]>();
            for (var i = 0; i <= len; i++)
            {
                spectrogram.Add(new float[_fftSize / 2 + 1]);
            }

            // spectrogram:

            for (int pos = 0, i = 0; pos + _windowSize < samples.Length; pos += _hopSize, i++)
            {
                _zeroblock.FastCopyTo(_re, _fftSize);
                samples.FastCopyTo(_re, _windowSize, pos);
                
                if (_window != WindowTypes.Rectangular)
                {
                    _re.ApplyWindow(_windowSamples);
                }

                _fft.PowerSpectrum(_re, spectrogram[i]);
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

        /// <summary>
        /// Method for computing a spectrogram as arrays of Magnitude and Phase.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>Magnitude-Phase spectrogram of the signal</returns>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(float[] samples)
        {
            // pre-allocate memory:

            var mag = new List<float[]>();
            var phase = new List<float[]>();

            var len = (samples.Length - _windowSize) / _hopSize;

            for (var i = 0; i <= len; i++)
            {
                mag.Add(new float[_fftSize / 2 + 1]);
                phase.Add(new float[_fftSize / 2 + 1]);
            }

            // magnitude-phase spectrogram:

            for (int pos = 0, i = 0; pos + _windowSize < samples.Length; pos += _hopSize, i++)
            {
                _zeroblock.FastCopyTo(_re, _fftSize);
                _zeroblock.FastCopyTo(_im, _fftSize);
                samples.FastCopyTo(_re, _windowSize, pos);

                if (_window != WindowTypes.Rectangular)
                {
                    _re.ApplyWindow(_windowSamples);
                }

                _fft.Direct(_re, _im);

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    mag[i][j] = (float)(Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]));
                    phase[i][j] = (float)(Math.Atan2(_im[j], _re[j]));
                }
            }

            return new MagnitudePhaseList { Magnitudes = mag, Phases = phase };
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <returns>Magnitude-Phase spectrogram of the signal</returns>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(DiscreteSignal signal)
        {
            return MagnitudePhaseSpectrogram(signal.Samples);
        }

        /// <summary>
        /// Reconstruct samples from magnitude-phase spectrogram
        /// </summary>
        /// <param name="spectrogram"></param>
        /// <returns></returns>
        public float[] ReconstructMagnitudePhase(MagnitudePhaseList spectrogram)
        {
            var spectraCount = spectrogram.Magnitudes.Count;
            var output = new float[spectraCount * _hopSize + _windowSize];

            var mag = spectrogram.Magnitudes;
            var phase = spectrogram.Phases;

            var pos = 0;
            for (var i = 0; i < spectraCount; i++)
            {
                if (_windowSize < _fftSize)
                {
                    _zeroblock.FastCopyTo(_re, _fftSize);
                    _zeroblock.FastCopyTo(_im, _fftSize);
                }

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    _re[j] = (float)(mag[i][j] * Math.Cos(phase[i][j]));
                    _im[j] = (float)(mag[i][j] * Math.Sin(phase[i][j]));
                    _re[_fftSize - 1 - j] = _re[j];
                    _im[_fftSize - 1 - j] = _im[j];
                }

                _fft.Inverse(_re, _im);

                // windowing and reconstruction

                for (var j = 0; j < _re.Length; j++)
                {
                    output[pos + j] += _re[j] * _windowSamples[j];
                }

                for (var j = 0; j < _hopSize; j++)
                {
                    output[pos + j] *= _gain;
                }

                pos += _hopSize;
            }

            for (var j = 0; j < _windowSize; j++)
            {
                output[pos + j] *= _gain;
            }

            return output;
        }
    }

    public struct MagnitudePhaseList
    {
        public List<float[]> Magnitudes { get; set; }
        public List<float[]> Phases { get; set; }
    }
}
