using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class representing Short-Time Fourier Transform.
    /// </summary>
    public class Stft
    {
        /// <summary>
        /// FFT size (number of samples).
        /// </summary>
        public int Size => _fftSize;
        private readonly int _fftSize;

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly RealFft _fft;

        /// <summary>
        /// Overlap size (number of samples).
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Window size (number of samples).
        /// </summary>
        private readonly int _windowSize;

        /// <summary>
        /// Window type.
        /// </summary>
        private readonly WindowType _window;

        /// <summary>
        /// Pre-computed samples of the window function.
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Construct STFT transformer.
        /// </summary>
        /// <param name="windowSize">Size of analysis window</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <param name="fftSize">Size of FFT</param>
        public Stft(int windowSize = 1024, int hopSize = 256, WindowType window = WindowType.Hann, int fftSize = 0)
        {
            _fftSize = fftSize >= windowSize ? fftSize : MathUtils.NextPowerOfTwo(windowSize);
            _fft = new RealFft(_fftSize);

            _hopSize = hopSize;
            _windowSize = windowSize;
            _window = window;
            _windowSamples = Window.OfType(_window, _windowSize);
        }

        /// <summary>
        /// Do STFT of an <paramref name="input"/>. 
        /// Returns list of computed spectra (real and imaginary parts) in time.
        /// </summary>
        /// <param name="input">Input data</param>
        public List<(float[], float[])> Direct(float[] input)
        {
            // pre-allocate memory:

            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var stft = new List<(float[], float[])>(len + 1);

            for (var i = 0; i < len; i++)
            {
                stft.Add((new float[_fftSize], new float[_fftSize]));
            }

            // stft:

            var windowedBuffer = new float[_fftSize];
            
            var pos = 0;

            for (var i = 0; i < len; pos += _hopSize, i++)
            {
                input.FastCopyTo(windowedBuffer, _windowSize, pos);

                windowedBuffer.ApplyWindow(_windowSamples);

                var (re, im) = stft[i];

                _fft.Direct(windowedBuffer, re, im);
            }

            // last (incomplete) frame:

            stft.Add((new float[_fftSize], new float[_fftSize]));

            Array.Clear(windowedBuffer, 0, _fftSize);
            input.FastCopyTo(windowedBuffer, input.Length - pos, pos);
            windowedBuffer.ApplyWindow(_windowSamples);
                        
            var (lre, lim) = stft.Last();

            _fft.Direct(windowedBuffer, lre, lim);

            return stft;
        }

        /// <summary>
        /// Do STFT of a <paramref name="signal"/>. 
        /// Returns list of computed spectra (real and imaginary parts) in time.
        /// </summary>
        /// <param name="signal">Input signal</param>
        public List<(float[], float[])> Direct(DiscreteSignal signal)
        {
            return Direct(signal.Samples);
        }

        /// <summary>
        /// Do Inverse STFT from list of spectra <paramref name="stft"/>.
        /// </summary>
        /// <param name="stft">List of spectra (real and imaginary parts)</param>
        /// <param name="perfectReconstruction">Perfect reconstruction mode</param>
        public float[] Inverse(List<(float[], float[])> stft, bool perfectReconstruction = true)
        {
            var spectraCount = stft.Count;
            var output = new float[spectraCount * _hopSize + _fftSize];

            var buf = new float[_fftSize];

            float gain;

            if (perfectReconstruction)
            {
                Guard.AgainstExceedance(_hopSize, _windowSize, "Hop size for perfect reconstruction", "window size");

                gain = 1f / _windowSize;
            }
            // simpler reconstruction of the signal
            // (with insignificant discrepancies in the beginning and in the end)
            else
            {
                gain = 1 / (_fftSize * _windowSamples.Select(w => w * w).Sum() / _hopSize);
            }


            var pos = 0;

            for (var i = 0; i < spectraCount; i++)
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
                    output[pos + j] *= gain;
                }

                pos += _hopSize;
            }

            for (var j = 0; j < _windowSize; j++)
            {
                output[pos + j] *= gain;
            }


            if (perfectReconstruction)      // additional normalization
            {
                float[] windowSummed = ComputeWindowSummed();

                var offset = _windowSize - _hopSize;

                for (int j = 0, k = output.Length - _hopSize - 1; j < offset; j++, k--)
                {
                    if (Math.Abs(windowSummed[j]) > 1e-30)
                    {
                        output[j] /= windowSummed[j];   // leftmost part of the signal
                        output[k] /= windowSummed[j];   // rightmost part of the signal
                    }
                }

                // main central part of the signal

                for (int j = offset, k = offset; j < output.Length - _windowSize; j++, k++)
                {
                    if (k == _windowSize) k = offset;

                    output[j] /= windowSummed[k];
                }
            }

            return output;
        }

        /// <summary>
        /// Helper method for ISTFT in 'perfect reconstruction' mode.
        /// </summary>
        /// <returns>Summed window coefficients</returns>
        private float[] ComputeWindowSummed()
        {
            var windowSummed = new float[_windowSize];

            for (var pos = 0; pos < _windowSize; pos += _hopSize)
            {
                for (var j = 0; pos + j < _windowSize; j++)
                {
                    windowSummed[pos + j] += _windowSamples[j] * _windowSamples[j];
                }
            }

            return windowSummed;
        }

        /// <summary>
        /// Compute spectrogram. 
        /// The spectrogram is essentially a list of power spectra in time.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="normalize">Normalize each spectrum</param>
        public List<float[]> Spectrogram(float[] input, bool normalize = true)
        {
            // pre-allocate memory:

            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var spectrogram = new List<float[]>(len + 1);

            for (var i = 0; i < len; i++)
            {
                spectrogram.Add(new float[_fftSize / 2 + 1]);
            }

            // spectrogram:

            var windowedBuffer = new float[_fftSize];
            
            var pos = 0;

            for (int i = 0; i < len; pos += _hopSize, i++)
            {
                input.FastCopyTo(windowedBuffer, _windowSize, pos);

                if (_window != WindowType.Rectangular)
                {
                    windowedBuffer.ApplyWindow(_windowSamples);
                }

                _fft.PowerSpectrum(windowedBuffer, spectrogram[i], normalize);
            }

            // last (incomplete) frame:

            Array.Clear(windowedBuffer, 0, _fftSize);
            input.FastCopyTo(windowedBuffer, input.Length - pos, pos);
            windowedBuffer.ApplyWindow(_windowSamples);

            spectrogram.Add(new float[_fftSize / 2 + 1]);

            _fft.PowerSpectrum(windowedBuffer, spectrogram.Last(), normalize);

            return spectrogram;
        }

        /// <summary>
        /// Compute spectrogram. 
        /// The spectrogram is essentially a list of power spectra in time.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="normalize">Normalize each spectrum</param>
        public List<float[]> Spectrogram(DiscreteSignal signal, bool normalize = true)
        {
            return Spectrogram(signal.Samples, normalize);
        }

        /// <summary>
        /// Compute averaged periodogram (used, for example, in Welch method). 
        /// This method is memory-efficient since it doesn't store all spectra in memory.
        /// </summary>
        /// <param name="input">Input data</param>
        public float[] AveragePeriodogram(float[] input)
        {
            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var spectrum = new float[_fftSize / 2 + 1];
            var periodogram = new float[_fftSize / 2 + 1];
            var windowedBuffer = new float[_fftSize];

            var pos = 0;

            for (var i = 0; i < len; pos += _hopSize, i++)
            {
                input.FastCopyTo(windowedBuffer, _windowSize, pos);

                if (_window != WindowType.Rectangular)
                {
                    windowedBuffer.ApplyWindow(_windowSamples);
                }

                _fft.PowerSpectrum(windowedBuffer, spectrum, false);

                for (var j = 0; j < periodogram.Length; j++)
                {
                    periodogram[j] += spectrum[j];
                }
            }

            // last (incomplete) frame:

            Array.Clear(windowedBuffer, 0, _fftSize);
            input.FastCopyTo(windowedBuffer, input.Length - pos, pos);
            windowedBuffer.ApplyWindow(_windowSamples);

            _fft.PowerSpectrum(windowedBuffer, spectrum, false);

            for (var j = 0; j < periodogram.Length; j++)
            {
                periodogram[j] += spectrum[j];    // add last spectrum
                periodogram[j] /= len + 1;        // and compute average right away
            }

            return periodogram;
        }

        /// <summary>
        /// Compute spectrogram in the form of list of magnitudes and phases from <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Input data</param>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(float[] input)
        {
            // pre-allocate memory:

            var len = input.Length >= _windowSize ? (input.Length - _windowSize) / _hopSize + 1 : 0;

            var mag = new List<float[]>(len + 1);
            var phase = new List<float[]>(len + 1);

            for (var i = 0; i < len; i++)
            {
                mag.Add(new float[_fftSize / 2 + 1]);
                phase.Add(new float[_fftSize / 2 + 1]);
            }

            // magnitude-phase spectrogram:
            
            var windowedBuffer = new float[_fftSize];
            var re = new float[_fftSize / 2 + 1];
            var im = new float[_fftSize / 2 + 1];

            var pos = 0;

            for (var i = 0; i < len; pos += _hopSize, i++)
            {
                input.FastCopyTo(windowedBuffer, _windowSize, pos);

                windowedBuffer.ApplyWindow(_windowSamples);

                _fft.Direct(windowedBuffer, re, im);

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    mag[i][j] = (float)Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                    phase[i][j] = (float)Math.Atan2(im[j], re[j]);
                }
            }

            // last (incomplete) frame:

            Array.Clear(windowedBuffer, 0, _fftSize);
            input.FastCopyTo(windowedBuffer, input.Length - pos, pos);
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

            return new MagnitudePhaseList { Magnitudes = mag, Phases = phase };
        }

        /// <summary>
        /// Compute spectrogram in the form of list of magnitudes and phases from <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Input signal</param>
        public MagnitudePhaseList MagnitudePhaseSpectrogram(DiscreteSignal signal)
        {
            return MagnitudePhaseSpectrogram(signal.Samples);
        }

        /// <summary>
        /// Reconstruct samples from <paramref name="spectrogram"/> in the form of list of magnitudes and phases.
        /// </summary>
        /// <param name="spectrogram">Spectrogram in the form of list of magnitudes and phases</param>
        /// <param name="perfectReconstruction">Perfect reconstruction mode</param>
        public float[] ReconstructMagnitudePhase(MagnitudePhaseList spectrogram, bool perfectReconstruction = true)
        {
            var spectraCount = spectrogram.Magnitudes.Count;
            var output = new float[spectraCount * _hopSize + _windowSize];

            var mag = spectrogram.Magnitudes;
            var phase = spectrogram.Phases;

            var buf = new float[_fftSize];
            var re = new float[_fftSize / 2 + 1];
            var im = new float[_fftSize / 2 + 1];

            float gain;
            
            if (perfectReconstruction)
            {
                Guard.AgainstExceedance(_hopSize, _windowSize, "Hop size for perfect reconstruction", "window size");

                gain = 1f / _windowSize;
            }
            // simpler reconstruction of the signal
            // (with insignificant discrepancies in the beginning and in the end)
            else
            {
                gain = 1 / (_fftSize * _windowSamples.Select(w => w * w).Sum() / _hopSize);
            }

            var pos = 0;

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
                    output[pos + j] *= gain;
                }

                pos += _hopSize;
            }

            for (var j = 0; j < _windowSize; j++)
            {
                output[pos + j] *= gain;
            }


            if (perfectReconstruction)      // additional normalization
            {
                float[] windowSummed = ComputeWindowSummed();

                var offset = _windowSize - _hopSize;

                for (int j = 0, k = output.Length - _hopSize - 1; j < offset; j++, k--)
                {
                    if (Math.Abs(windowSummed[j]) > 1e-30)
                    {
                        output[j] /= windowSummed[j];   // leftmost part of the signal
                        output[k] /= windowSummed[j];   // rightmost part of the signal
                    }
                }

                // main central part of the signal

                for (int j = offset, k = offset; j < output.Length - _windowSize; j++, k++)
                {
                    if (k == _windowSize) k = offset;

                    output[j] /= windowSummed[k];
                }
            }

            return output;
        }
    }

    /// <summary>
    /// Spectrogram in the form of list of magnitudes and phases.
    /// </summary>
    public struct MagnitudePhaseList
    {
        /// <summary>
        /// Gets or sets list of magnitudes.
        /// </summary>
        public List<float[]> Magnitudes { get; set; }

        /// <summary>
        /// Gets or sets list of phases.
        /// </summary>
        public List<float[]> Phases { get; set; }
    }
}
