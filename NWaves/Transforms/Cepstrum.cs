using System;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for direct and inverse cepstrum transforms
    /// </summary>
    public class Cepstrum
    {
        /// <summary>
        /// Size of cepstrum
        /// </summary>
        private readonly int _cepstrumSize;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

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
        /// Constructor with necessary parameters
        /// </summary>
        /// <param name="cepstrumSize"></param>
        /// <param name="fftSize"></param>
        public Cepstrum(int cepstrumSize, int fftSize = 512)
        {
            var pow = (int)Math.Log(fftSize, 2);
            if (fftSize != 1 << pow)
            {
                throw new ArgumentException("FFT size must be a power of 2!");
            }

            _fftSize = fftSize;
            _fft = new Fft(_fftSize);
            _realSpectrum = new double[fftSize];
            _imagSpectrum = new double[fftSize];
            _zeroblock = new double[fftSize];

            _cepstrumSize = cepstrumSize;
        }

        /// <summary>
        /// Method for computing real cepstrum from array of samples
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="cepstrum"></param>
        /// <returns></returns>
        public void Direct(double[] samples, double[] cepstrum)
        {
            FastCopy.ToExistingArray(_zeroblock, _realSpectrum, _fftSize);
            FastCopy.ToExistingArray(_zeroblock, _imagSpectrum, _fftSize);
            FastCopy.ToExistingArray(samples, _realSpectrum, Math.Min(samples.Length, _fftSize));

            // complex fft
            _fft.Direct(_realSpectrum, _imagSpectrum);

            // complex logarithm
            for (var i = 0; i < _fftSize; i++)
            {
                _realSpectrum[i] = Math.Log(Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]));
            }

            // complex ifft
            _fft.Inverse(_realSpectrum, _imagSpectrum);

            // take real truncated part
            FastCopy.ToExistingArray(_realSpectrum, cepstrum, _cepstrumSize);
        }

        /// <summary>
        /// Method for computing real cepstrum of a signal
        /// </summary>
        /// <param name="signal"></param>
        /// <returns>Cepstrum signal</returns>
        public DiscreteSignal Direct(DiscreteSignal signal)
        {
            var cepstrum = new double[_cepstrumSize];
            Direct(signal.Samples, cepstrum);
            return new DiscreteSignal(signal.SamplingRate, cepstrum);
        }
    }
}
