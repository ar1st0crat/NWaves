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
        /// FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Intermediate buffer storing real parts of spectrum
        /// </summary>
        private readonly float[] _realSpectrum;

        /// <summary>
        /// Intermediate buffer storing imaginary parts of spectrum
        /// </summary>
        private readonly float[] _imagSpectrum;
        
        /// <summary>
        /// Constructor with necessary parameters
        /// </summary>
        /// <param name="cepstrumSize"></param>
        /// <param name="fftSize"></param>
        public CepstralTransform(int cepstrumSize, int fftSize = 512)
        {
            _fft = new Fft(fftSize);

            _cepstrumSize = cepstrumSize;

            _realSpectrum = new float[fftSize];
            _imagSpectrum = new float[fftSize];
        }

        /// <summary>
        /// Method for computing real cepstrum from array of samples
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="cepstrum"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public void Direct(float[] samples, float[] cepstrum, bool power = false)
        {
            // complex fft

            _fft.PowerSpectrum(samples, _realSpectrum, false);


            // logarithm of power spectrum

            for (var i = 0; i < _realSpectrum.Length; i++)
            {
                _realSpectrum[i] = (float)Math.Log10(_realSpectrum[i] + float.Epsilon);
                _imagSpectrum[i] = 0.0f;
            }


            // complex ifft

            _fft.Inverse(_realSpectrum, _imagSpectrum);


            // take truncated part

            if (power)
            {
                for (var i = 0; i < _cepstrumSize; i++)
                {
                    cepstrum[i] = (_realSpectrum[i] * _realSpectrum[i] + 
                                   _imagSpectrum[i] * _imagSpectrum[i]) / _realSpectrum.Length;
                }
            }
            else
            {
                _realSpectrum.FastCopyTo(cepstrum, _cepstrumSize);
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
            var cepstrum = new float[_cepstrumSize];
            Direct(signal.Samples, cepstrum, power);
            return new DiscreteSignal(signal.SamplingRate, cepstrum);
        }

        /// <summary>
        /// Method for computing inverse cepstrum
        /// </summary>
        /// <param name="cepstrum"></param>
        /// <param name="samples"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public void Inverse(float[] cepstrum, float[] samples, bool power = false)
        {
            if (cepstrum.Length != _realSpectrum.Length)
            {
                throw new ArgumentException("");
            }

            for (var i = 0; i < _realSpectrum.Length; i++)
            {
                _realSpectrum[i] = cepstrum[i];
                _imagSpectrum[i] = 0.0f;
            }

            if (power)
            {
                for (var i = 0; i < _realSpectrum.Length; i++)
                {
                    _realSpectrum[i] = (float) Math.Sqrt(_realSpectrum[i]) * _realSpectrum.Length;
                }
            }

            // FFT

            _fft.Direct(_realSpectrum, _imagSpectrum);

            // Pow ("inverse" logarithm)

            for (var i = 0; i < _realSpectrum.Length; i++)
            {
                samples[i] = (float)Math.Sqrt(Math.Pow(_realSpectrum[i], 10));
                _imagSpectrum[i] = 0.0f;
            }
            
            // IFFT
            
            _fft.Inverse(samples, _imagSpectrum);
        }

        /// <summary>
        /// Method for computing inverse cepstrum
        /// </summary>
        /// <param name="cepstrum"></param>
        /// <param name="power"></param>
        /// <returns>Cepstrum signal</returns>
        public DiscreteSignal Inverse(DiscreteSignal cepstrum, bool power = false)
        {
            var output = new float[_realSpectrum.Length];
            Inverse(cepstrum.Samples, output, power);
            return new DiscreteSignal(cepstrum.SamplingRate, output);
        }
    }
}
