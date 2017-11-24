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
        private readonly double[] _realSpectrum;

        /// <summary>
        /// Intermediate buffer storing imaginary parts of spectrum
        /// </summary>
        private readonly double[] _imagSpectrum;

        /// <summary>
        /// Just a buffer with zeros for quick memset
        /// </summary>
        private readonly double[] _zeroblock;

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
            _realSpectrum = new double[fftSize];
            _imagSpectrum = new double[fftSize];
            _zeroblock = new double[fftSize];
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
        public void MagnitudeSpectrum(double[] samples, double[] spectrum, bool normalize = false)
        {
            FastCopy.ToExistingArray(_zeroblock, _realSpectrum, _fftSize);
            FastCopy.ToExistingArray(_zeroblock, _imagSpectrum, _fftSize);
            FastCopy.ToExistingArray(samples, _realSpectrum, Math.Min(samples.Length, _fftSize));

            Direct(_realSpectrum, _imagSpectrum, _fftSize);

            if (normalize)
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize;
                }
            }
            else
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]);
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
        public void PowerSpectrum(double[] samples, double[] spectrum, bool normalize = true)
        {
            FastCopy.ToExistingArray(_zeroblock, _realSpectrum, _fftSize);
            FastCopy.ToExistingArray(_zeroblock, _imagSpectrum, _fftSize);
            FastCopy.ToExistingArray(samples, _realSpectrum, Math.Min(samples.Length, _fftSize));

            Direct(_realSpectrum, _imagSpectrum, _fftSize);

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
        /// Log power spectrum:
        /// 
        ///     spectrum = 20 * log10(re * re + im * im)
        /// 
        /// </summary>
        /// <param name="samples">Array of samples (samples parts)</param>
        /// <param name="spectrum">Log-power spectrum</param>
        /// <returns>Left half of the log-power spectrum</returns>
        public void LogPowerSpectrum(double[] samples, double[] spectrum)
        {
            FastCopy.ToExistingArray(_zeroblock, _realSpectrum, _fftSize);
            FastCopy.ToExistingArray(_zeroblock, _imagSpectrum, _fftSize);
            FastCopy.ToExistingArray(samples, _realSpectrum, Math.Min(samples.Length, _fftSize));

            Direct(_realSpectrum, _imagSpectrum, _fftSize);

            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = 20 * Math.Log10(_realSpectrum[i] * _realSpectrum[i] + 
                                              _imagSpectrum[i] * _imagSpectrum[i] + 
                                              double.Epsilon);
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
            var spectrum = new double[_fftSize / 2 + 1];
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
            var spectrum = new double[_fftSize / 2 + 1];
            PowerSpectrum(signal.Samples, spectrum, normalize);
            return new DiscreteSignal(signal.SamplingRate, spectrum);
        }

        /// <summary>
        /// Overloaded method for DiscreteSignal as an input
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public DiscreteSignal LogPowerSpectrum(DiscreteSignal signal)
        {
            var spectrum = new double[_fftSize / 2 + 1];
            LogPowerSpectrum(signal.Samples, spectrum);
            return new DiscreteSignal(signal.SamplingRate, spectrum);
        }
    }
}
