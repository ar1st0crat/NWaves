using NWaves.Signals;
using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for direct and inverse Fast Fourier Transforms
    /// and postprocessing: magnitude spectrum, power spectrum, logpower spectrum.
    /// </summary>
    public class Fft
    {
        /// <summary>
        /// The size of FFT
        /// </summary>
        public int Size => _fftSize;
        private readonly int _fftSize;

        /// <summary>
        /// Precomputed cosines
        /// </summary>
        private readonly float[] _cosTbl;

        /// <summary>
        /// Precomputed sines
        /// </summary>
        private readonly float[] _sinTbl;

        /// <summary>
        /// Intermediate buffer storing real parts of spectrum
        /// </summary>
        private readonly float[] _realSpectrum;

        /// <summary>
        /// Intermediate buffer storing imaginary parts of spectrum
        /// </summary>
        private readonly float[] _imagSpectrum;

        /// <summary>
        /// Constructor accepting the size of FFT
        /// </summary>
        /// <param name="fftSize">Size of FFT</param>
        public Fft(int fftSize = 512)
        {
            Guard.AgainstNotPowerOfTwo(fftSize, "FFT size");

            _fftSize = fftSize;
            _realSpectrum = new float[fftSize];
            _imagSpectrum = new float[fftSize];

            var tblSize = (int)Math.Log(fftSize, 2);

            _cosTbl = new float[tblSize];
            _sinTbl = new float[tblSize];

            for (int i = 1, pos = 0; i < _fftSize; i *= 2, pos++)
            {
                _cosTbl[pos] = (float)Math.Cos(2 * Math.PI * i / _fftSize);
                _sinTbl[pos] = (float)Math.Sin(2 * Math.PI * i / _fftSize);
            }
        }

        /// <summary>
        /// Fast Fourier Transform algorithm
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Direct(float[] re, float[] im)
        {
            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0f;
                var u2 = 0.0f;
                var c = _cosTbl[ti];
                var s = -_sinTbl[ti];
                ti++;
                for (var j = 0; j < l; j++)
                {
                    for (var i = j; i < _fftSize; i += L)
                    {
                        var p = i + l;
                        var t1 = re[i] + re[p];
                        var t2 = im[i] + im[p];
                        var t3 = re[i] - re[p];
                        var t4 = im[i] - im[p];
                        re[p] = t3 * u1 - t4 * u2;
                        im[p] = t4 * u1 + t3 * u2;
                        re[i] = t1;
                        im[i] = t2;
                    }
                    var u3 = u1 * c - u2 * s;
                    u2 = u2 * c + u1 * s;
                    u1 = u3;
                }
                L >>= 1;
            }
            for (int i = 0, j = 0; i < S; i++)
            {
                if (i > j)
                {
                    var t1 = re[j];
                    var t2 = im[j];
                    re[j] = re[i];
                    im[j] = im[i];
                    re[i] = t1;
                    im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }
        }

        /// <summary>
        /// Inverse Fast Fourier Transform algorithm
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Inverse(float[] re, float[] im)
        {
            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0f;
                var u2 = 0.0f;
                var c = _cosTbl[ti];
                var s = _sinTbl[ti];
                ti++;
                for (var j = 0; j < l; j++)
                {
                    for (var i = j; i < _fftSize; i += L)
                    {
                        var p = i + l;
                        var t1 = re[i] + re[p];
                        var t2 = im[i] + im[p];
                        var t3 = re[i] - re[p];
                        var t4 = im[i] - im[p];
                        re[p] = t3 * u1 - t4 * u2;
                        im[p] = t4 * u1 + t3 * u2;
                        re[i] = t1;
                        im[i] = t2;
                    }
                    var u3 = u1 * c - u2 * s;
                    u2 = u2 * c + u1 * s;
                    u1 = u3;
                }
                L >>= 1;
            }
            for (int i = 0, j = 0; i < S; i++)
            {
                if (i > j)
                {
                    var t1 = re[j];
                    var t2 = im[j];
                    re[j] = re[i];
                    im[j] = im[i];
                    re[i] = t1;
                    im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
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
            Array.Clear(_realSpectrum, 0, _fftSize);
            Array.Clear(_imagSpectrum, 0, _fftSize);

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
            Array.Clear(_realSpectrum, 0, _fftSize);
            Array.Clear(_imagSpectrum, 0, _fftSize);

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

        /// <summary>
        /// FFT shift (in-place)
        /// </summary>
        /// <param name="samples"></param>
        public static void Shift(float[] samples)
        {
            if ((samples.Length & 1) == 1)
            {
                throw new ArgumentException("FFT shift is not supported for arrays with odd lengths");
            }

            var mid = samples.Length / 2;

            for (var i = 0; i < samples.Length / 2; i++)
            {
                var shift = i + mid;
                var tmp = samples[i];
                samples[i] = samples[shift];
                samples[shift] = tmp;
            }
        }
    }
}
