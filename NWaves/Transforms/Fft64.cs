using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for direct and inverse Fast Fourier Transforms
    /// and postprocessing: magnitude spectrum, power spectrum, logpower spectrum.
    /// </summary>
    public class Fft64
    {
        /// <summary>
        /// The size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Constructor accepting the size of FFT
        /// </summary>
        /// <param name="fftSize">Size of FFT</param>
        public Fft64(int fftSize = 512)
        {
            var pow = (int)Math.Log(fftSize, 2);
            if (fftSize != 1 << pow)
            {
                throw new ArgumentException("FFT size must be a power of 2!");
            }

            _fftSize = fftSize;

            var tblSize = (int)Math.Log(fftSize, 2);
            _cosTbl = new double[tblSize];
            _sinTbl = new double[tblSize];

            var pos = 0;
            for (var i = 1; i < _fftSize; i *= 2)
            {
                _cosTbl[pos] = Math.Cos(2 * Math.PI * i / _fftSize);
                _sinTbl[pos] = Math.Sin(2 * Math.PI * i / _fftSize);
                pos++;
            }
        }

        /// <summary>
        /// Fast Fourier Transform algorithm
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Direct(double[] re, double[] im)
        {
            double t1, t2;
            int i, j;
            int L, M, S;

            L = _fftSize;
            M = _fftSize >> 1;
            S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
                var c = _cosTbl[ti];
                var s = -_sinTbl[ti];
                ti++;
                for (j = 0; j < l; j++)
                {
                    for (i = j; i < _fftSize; i += L)
                    {
                        var p = i + l;
                        t1 = re[i] + re[p];
                        t2 = im[i] + im[p];
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
            j = 0;
            for (i = 0; i < S; i++)
            {
                if (i > j)
                {
                    t1 = re[j];
                    t2 = im[j];
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
        public void Inverse(double[] re, double[] im)
        {
            double t1, t2;
            int i, j;
            int L, M, S;

            L = _fftSize;
            M = _fftSize >> 1;
            S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
                var c = _cosTbl[ti];
                var s = _sinTbl[ti];
                ti++;
                for (j = 0; j < l; j++)
                {
                    for (i = j; i < _fftSize; i += L)
                    {
                        var p = i + l;
                        t1 = re[i] + re[p];
                        t2 = im[i] + im[p];
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
            j = 0;
            for (i = 0; i < S; i++)
            {
                if (i > j)
                {
                    t1 = re[j];
                    t2 = im[j];
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

        private readonly double[] _cosTbl;
        private readonly double[] _sinTbl;

        ///// <summary>
        ///// Magnitude spectrum:
        ///// 
        /////     spectrum = sqrt(re * re + im * im)
        ///// 
        ///// </summary>
        ///// <param name="samples">Array of samples (samples parts)</param>
        ///// <param name="spectrum">Magnitude spectrum</param>
        ///// <param name="normalize">Normalization flag</param>
        //public void MagnitudeSpectrum(double[] samples, double[] spectrum, bool normalize = false)
        //{
        //    _zeroblock.FastCopyTo(_realSpectrum, _fftSize);
        //    _zeroblock.FastCopyTo(_imagSpectrum, _fftSize);
        //    samples.FastCopyTo(_realSpectrum, Math.Min(samples.Length, _fftSize));

        //    Direct(_realSpectrum, _imagSpectrum);

        //    if (normalize)
        //    {
        //        for (var i = 0; i < spectrum.Length; i++)
        //        {
        //            spectrum[i] = Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize;
        //        }
        //    }
        //    else
        //    {
        //        for (var i = 0; i < spectrum.Length; i++)
        //        {
        //            spectrum[i] = Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Power spectrum (normalized by default):
        ///// 
        /////     spectrum =   (re * re + im * im) / fftSize
        ///// 
        ///// </summary>
        ///// <param name="samples">Array of samples (samples parts)</param>
        ///// <param name="spectrum">Power spectrum</param>
        ///// <param name="normalize">Normalization flag</param>
        //public void PowerSpectrum(double[] samples, double[] spectrum, bool normalize = true)
        //{
        //    _zeroblock.FastCopyTo(_realSpectrum, _fftSize);
        //    _zeroblock.FastCopyTo(_imagSpectrum, _fftSize);
        //    samples.FastCopyTo(_realSpectrum, Math.Min(samples.Length, _fftSize));

        //    Direct(_realSpectrum, _imagSpectrum);

        //    if (normalize)
        //    {
        //        for (var i = 0; i < spectrum.Length; i++)
        //        {
        //            spectrum[i] = (_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize;
        //        }
        //    }
        //    else
        //    {
        //        for (var i = 0; i < spectrum.Length; i++)
        //        {
        //            spectrum[i] = _realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i];
        //        }
        //    }
        //}
    }
}
