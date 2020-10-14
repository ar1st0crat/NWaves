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
        private readonly RealFft _fft;

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
        /// Constructor with necessary parameters
        /// </summary>
        /// <param name="windowSize">Size of window</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <param name="fftSize">Size of FFT</param>
        public Stft(int windowSize = 1024, int hopSize = 256, WindowTypes window = WindowTypes.Hann, int fftSize = 0)
        {
            _fftSize = fftSize >= windowSize ? fftSize : MathUtils.NextPowerOfTwo(windowSize);
            _fft = new RealFft(_fftSize);

            _hopSize = hopSize;
            _windowSize = windowSize;
            _window = window;
            _windowSamples = Window.OfType(_window, _windowSize);

            _gain = 1 / (_fftSize * _windowSamples.Select(w => w * w).Sum() / _hopSize);
        }

        /// <summary>
        /// Method for computing direct STFT of a signal block.
        /// STFT (spectrogram) is essentially the list of spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>STFT of the signal</returns>
        public List<(float[], float[])> Direct(float[] samples)
        {
            // pre-allocate memory:

            var len = (samples.Length - _windowSize) / _hopSize + 1;

            var stft = new List<(float[], float[])>(len);

            for (int i = 0; i < len; i++)
            {
                stft.Add((new float[_fftSize], new float[_fftSize]));
            }

            // stft:

            var windowedBuffer = new float[_fftSize];

            int pos = 0;

            for (int i = 0; pos + _windowSize < samples.Length; pos += _hopSize, i++)
            {
                samples.FastCopyTo(windowedBuffer, _windowSize, pos);

                windowedBuffer.ApplyWindow(_windowSamples);

                var (re, im) = stft[i];

                _fft.Direct(windowedBuffer, re, im);
            }

            // last (incomplete) frame:

            if (pos < samples.Length)
            {
                Array.Clear(windowedBuffer, 0, _fftSize);
                samples.FastCopyTo(windowedBuffer, samples.Length - pos, pos);
                windowedBuffer.ApplyWindow(_windowSamples);

                stft.Add((new float[_fftSize], new float[_fftSize]));
                var (re, im) = stft.Last();

                _fft.Direct(windowedBuffer, re, im);
            }

            return stft;
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">The signal under analysis</param>
        /// <returns>STFT of the signal</returns>
        public List<(float[], float[])> Direct(DiscreteSignal signal)
        {
            return Direct(signal.Samples);
        }

        /// <summary>
        /// Inverse STFT
        /// </summary>
        /// <param name="stft">Result of Direct STFT</param>
        /// <param name="perfectReconstruction"></param>
        /// <returns>ISTFT</returns>
        public float[] Inverse(List<(float[], float[])> stft, bool perfectReconstruction = false)
        {
            var spectraCount = stft.Count;
            var output = new float[spectraCount * _hopSize + _fftSize];

            var buf = new float[_fftSize];

            int pos = 0;

            if (perfectReconstruction)
            {
                var gain = 1f / _windowSize;

                var windowSum = new float[output.Length];

                for (int i = 0; i < spectraCount; i++)
                {
                    var (re, im) = stft[i];

                    _fft.Inverse(re, im, buf);

                    // windowing and reconstruction

                    for (var j = 0; j < _windowSize; j++)
                    {
                        output[pos + j] += buf[j] * _windowSamples[j];
                        windowSum[pos + j] += _windowSamples[j] * _windowSamples[j];
                    }

                    for (var j = 0; j < _hopSize; j++)
                    {
                        output[pos + j] *= gain;
                    }

                    pos += _hopSize;
                }

                for (var j = 0; j < _windowSize; j++)
                {
                    output[pos + j] *= gain;
                }

                for (var j = 0; j < output.Length; j++)
                {
                    if (Math.Abs(windowSum[j]) > 1e-30)
                    {
                        output[j] /= windowSum[j];
                    }
                }
            }
            else
            {
                for (int i = 0; i < spectraCount; i++)
                {
                    var (re, im) = stft[i];

                    _fft.Inverse(re, im, buf);

                    // windowing and reconstruction

                    for (var j = 0; j < _windowSize; j++)
                    {
                        output[pos + j] += buf[j] * _windowSamples[j];
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
            }

            return output;
        }

        /// <summary>
        /// Method for computing a spectrogram.
        /// The spectrogram is essentially a list of power spectra in time.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <param name="normalize">Normalize each spectrum</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<float[]> Spectrogram(float[] samples, bool normalize = true)
        {
            // pre-allocate memory:

            var len = (samples.Length - _windowSize) / _hopSize;

            var spectrogram = new List<float[]>(len + 1);

            for (var i = 0; i <= len; i++)
            {
                spectrogram.Add(new float[_fftSize / 2 + 1]);
            }

            // spectrogram:

            var windowedBuffer = new float[_fftSize];
            
            int pos = 0;

            for (int i = 0; pos + _windowSize < samples.Length; pos += _hopSize, i++)
            {
                samples.FastCopyTo(windowedBuffer, _windowSize, pos);

                if (_window != WindowTypes.Rectangular)
                {
                    windowedBuffer.ApplyWindow(_windowSamples);
                }

                _fft.PowerSpectrum(windowedBuffer, spectrogram[i], normalize);
            }

            // last (incomplete) frame:

            if (pos < samples.Length)
            {
                Array.Clear(windowedBuffer, 0, _fftSize);
                samples.FastCopyTo(windowedBuffer, samples.Length - pos, pos);
                windowedBuffer.ApplyWindow(_windowSamples);

                spectrogram.Add(new float[_fftSize / 2 + 1]);

                _fft.PowerSpectrum(windowedBuffer, spectrogram.Last(), normalize);
            }

            return spectrogram;
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="normalize">Normalize each spectrum</param>
        /// <returns>Spectrogram of the signal</returns>
        public List<float[]> Spectrogram(DiscreteSignal signal, bool normalize = true)
        {
            return Spectrogram(signal.Samples, normalize);
        }

        /// <summary>
        /// Method for computing a spectrogram as arrays of Magnitude and Phase.
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <returns>Magnitude-Phase spectrogram of the signal</returns>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(float[] samples)
        {
            // pre-allocate memory:

            var len = (samples.Length - _windowSize) / _hopSize;

            var mag = new List<float[]>(len + 1);
            var phase = new List<float[]>(len + 1);

            for (var i = 0; i <= len; i++)
            {
                mag.Add(new float[_fftSize / 2 + 1]);
                phase.Add(new float[_fftSize / 2 + 1]);
            }

            // magnitude-phase spectrogram:
            
            var windowedBuffer = new float[_fftSize];
            var re = new float[_fftSize / 2 + 1];
            var im = new float[_fftSize / 2 + 1];

            int pos = 0;

            for (int i = 0; pos + _windowSize < samples.Length; pos += _hopSize, i++)
            {
                samples.FastCopyTo(windowedBuffer, _windowSize, pos);

                windowedBuffer.ApplyWindow(_windowSamples);

                _fft.Direct(windowedBuffer, re, im);

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    mag[i][j] = (float)Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                    phase[i][j] = (float)Math.Atan2(im[j], re[j]);
                }
            }

            // last (incomplete) frame:

            if (pos < samples.Length)
            {
                Array.Clear(windowedBuffer, 0, _fftSize);
                samples.FastCopyTo(windowedBuffer, samples.Length - pos, pos);
                windowedBuffer.ApplyWindow(_windowSamples);

                mag.Add(new float[_fftSize / 2 + 1]);
                phase.Add(new float[_fftSize / 2 + 1]);

                _fft.Direct(windowedBuffer, re, im);

                var m = mag.Last();
                var p = phase.Last();

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    m[j] = (float)Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                    p[j] = (float)Math.Atan2(im[j], re[j]);
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
        /// <param name="perfectReconstruction"></param>
        /// <returns></returns>
        public float[] ReconstructMagnitudePhase(MagnitudePhaseList spectrogram, bool perfectReconstruction = false)
        {
            var spectraCount = spectrogram.Magnitudes.Count;
            var output = new float[spectraCount * _hopSize + _windowSize];

            var mag = spectrogram.Magnitudes;
            var phase = spectrogram.Phases;

            var buf = new float[_fftSize];
            var re = new float[_fftSize / 2 + 1];
            var im = new float[_fftSize / 2 + 1];

            int pos = 0;

            if (perfectReconstruction)
            {
                var gain = 1f / _windowSize;

                var windowSum = new float[output.Length];

                for (var i = 0; i < spectraCount; i++)
                {
                    for (var j = 0; j <= _fftSize / 2; j++)
                    {
                        re[j] = (float)(mag[i][j] * Math.Cos(phase[i][j]));
                        im[j] = (float)(mag[i][j] * Math.Sin(phase[i][j]));
                    }

                    _fft.Inverse(re, im, buf);

                    // windowing and reconstruction

                    for (var j = 0; j < _windowSize; j++)
                    {
                        output[pos + j] += buf[j] * _windowSamples[j];
                        windowSum[pos + j] += _windowSamples[j] * _windowSamples[j];
                    }

                    for (var j = 0; j < _hopSize; j++)
                    {
                        output[pos + j] *= gain;
                    }

                    pos += _hopSize;
                }

                for (var j = 0; j < _windowSize; j++)
                {
                    output[pos + j] *= gain;
                }

                for (var j = 0; j < output.Length; j++)
                {
                    if (Math.Abs(windowSum[j]) > 1e-30)
                    {
                        output[j] /= windowSum[j];
                    }
                }
            }
            else
            {
                for (var i = 0; i < spectraCount; i++)
                {
                    for (var j = 0; j <= _fftSize / 2; j++)
                    {
                        re[j] = (float)(mag[i][j] * Math.Cos(phase[i][j]));
                        im[j] = (float)(mag[i][j] * Math.Sin(phase[i][j]));
                    }

                    _fft.Inverse(re, im, buf);

                    // windowing and reconstruction

                    for (var j = 0; j < _windowSize; j++)
                    {
                        output[pos + j] += buf[j] * _windowSamples[j];
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
