using System;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for direct and inverse Fast Fourier Transforms
    /// and postprocessing: magnitude spectrum, power spectrum, logpower spectrum.
    /// </summary>
    public partial class Fft
    {
        /// <summary>
        /// The size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Intermediate buffer storing real parts of spectrum
        /// </summary>
        private readonly float[] _realSpectrum;

        /// <summary>
        /// Intermediate buffer storing imaginary parts of spectrum
        /// </summary>
        private readonly float[] _imagSpectrum;

        /// <summary>
        /// Just a buffer with zeros for quick memset
        /// </summary>
        private readonly float[] _zeroblock;

        /// <summary>
        /// Constructor accepting the size of FFT
        /// </summary>
        /// <param name="fftSize">Size of FFT</param>
        public Fft(int fftSize = 512)
        {
            var pow = (int) Math.Log(fftSize, 2);
            if (fftSize != 1 << pow)
            {
                throw new ArgumentException("FFT size must be a power of 2!");
            }

            _fftSize = fftSize;
            _realSpectrum = new float[fftSize];
            _imagSpectrum = new float[fftSize];
            _zeroblock = new float[fftSize];

            var tblSize = (int)Math.Log(fftSize, 2);
            _cosTbl = new float[tblSize];
            _sinTbl = new float[tblSize];

            var pos = 0;
            for (var i = 1; i < _fftSize; i *= 2)
            {
                _cosTbl[pos] = (float)Math.Cos(2 * Math.PI * i / _fftSize);
                _sinTbl[pos] = (float)Math.Sin(2 * Math.PI * i / _fftSize);
                pos++;
            }
        }

        /// <summary>
        /// Magnitude spectrum:
        /// 
        ///     spectrum = sqrt(re * re + im * im)
        /// 
        /// </summary>
        /// <param name="samples">Array of samples (samples parts)</param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <param name="normalize">Normalization flag</param>
        public void MagnitudeSpectrum(float[] samples, float[] spectrum, bool normalize = false)
        {
            _zeroblock.FastCopyTo(_realSpectrum, _fftSize);
            _zeroblock.FastCopyTo(_imagSpectrum, _fftSize);
            samples.FastCopyTo(_realSpectrum, Math.Min(samples.Length, _fftSize));

            Direct(_realSpectrum, _imagSpectrum);

            if (normalize)
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = (float)(Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize);
                }
            }
            else
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = (float)(Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]));
                }
            }
        }

        /// <summary>
        /// Power spectrum (normalized by default):
        /// 
        ///     spectrum =   (re * re + im * im) / fftSize
        /// 
        /// </summary>
        /// <param name="samples">Array of samples (samples parts)</param>
        /// <param name="spectrum">Power spectrum</param>
        /// <param name="normalize">Normalization flag</param>
        public void PowerSpectrum(float[] samples, float[] spectrum, bool normalize = true)
        {
            _zeroblock.FastCopyTo(_realSpectrum, _fftSize);
            _zeroblock.FastCopyTo(_imagSpectrum, _fftSize);
            samples.FastCopyTo(_realSpectrum, Math.Min(samples.Length, _fftSize));

            Direct(_realSpectrum, _imagSpectrum);

            if (normalize)
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = (_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize;
                }
            }
            else
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = _realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i];
                }
            }
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        public DiscreteSignal MagnitudeSpectrum(DiscreteSignal signal, bool normalize = false)
        {
            var spectrum = new float[_fftSize / 2 + 1];
            MagnitudeSpectrum(signal.Samples, spectrum, normalize);
            return new DiscreteSignal(signal.SamplingRate, spectrum);
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        public DiscreteSignal PowerSpectrum(DiscreteSignal signal, bool normalize = true)
        {
            var spectrum = new float[_fftSize / 2 + 1];
            PowerSpectrum(signal.Samples, spectrum, normalize);
            return new DiscreteSignal(signal.SamplingRate, spectrum);
        }
    }
}
