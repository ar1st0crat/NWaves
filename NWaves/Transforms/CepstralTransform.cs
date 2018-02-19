using System;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for direct and inverse cepstrum transforms
    /// </summary>
    public class CepstralTransform
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
        /// FFT transformer
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
        public CepstralTransform(int cepstrumSize, int fftSize = 512)
        {
            _fft = new Fft(fftSize);
            _fftSize = fftSize;
            _cepstrumSize = cepstrumSize;

            _realSpectrum = new double[fftSize];
            _imagSpectrum = new double[fftSize];
            _zeroblock = new double[fftSize];
        }

        /// <summary>
        /// Method for computing real cepstrum from array of samples
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="cepstrum"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public void Direct(double[] samples, double[] cepstrum, bool power = false)
        {
            // complex fft

            _fft.PowerSpectrum(samples, _realSpectrum, false);


            // logarithm of power spectrum

            for (var i = 0; i < _fftSize; i++)
            {
                _realSpectrum[i] = Math.Log10(_realSpectrum[i] + double.Epsilon);
                _imagSpectrum[i] = 0.0;
            }


            // complex ifft

            _fft.Inverse(_realSpectrum, _imagSpectrum);


            // take truncated part

            if (power)
            {
                for (var i = 0; i < _cepstrumSize; i++)
                {
                    cepstrum[i] = (_realSpectrum[i] * _realSpectrum[i] + 
                                   _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize;
                }
            }
            else
            {
                FastCopy.ToExistingArray(_realSpectrum, cepstrum, _cepstrumSize);
            }
        }

        /// <summary>
        /// Method for computing real cepstrum of a signal
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="power"></param>
        /// <returns>Cepstrum signal</returns>
        public DiscreteSignal Direct(DiscreteSignal signal, bool power = false)
        {
            var cepstrum = new double[_cepstrumSize];
            Direct(signal.Samples, cepstrum, power);
            return new DiscreteSignal(signal.SamplingRate, cepstrum);
        }
    }
}
