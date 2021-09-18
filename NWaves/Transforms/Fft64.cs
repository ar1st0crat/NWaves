using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// <para>Class representing Complex Fast Fourier Transform for 64-bit data:</para>
    /// <list type="bullet">
    ///     <item>Direct FFT</item>
    ///     <item>Inverse FFT</item>
    ///     <item>Magnitude spectrum</item>
    ///     <item>Power spectrum</item>
    /// </list>
    /// </summary>
    public class Fft64
    {
        /// <summary>
        /// FFT size.
        /// </summary>
        public int Size => _fftSize;
        private readonly int _fftSize;

        /// <summary>
        /// Precomputed cosines.
        /// </summary>
        private readonly double[] _cosTbl;

        /// <summary>
        /// Precomputed sines.
        /// </summary>
        private readonly double[] _sinTbl;

        /// <summary>
        /// Construct FFT transformer.
        /// </summary>
        /// <param name="fftSize">FFT size</param>
        public Fft64(int fftSize = 512)
        {
            Guard.AgainstNotPowerOfTwo(fftSize, "FFT size");

            _fftSize = fftSize;

            var tblSize = (int)Math.Log(fftSize, 2);

            _cosTbl = new double[tblSize];
            _sinTbl = new double[tblSize];

            for (int i = 1, pos = 0; i < _fftSize; i *= 2, pos++)
            {
                _cosTbl[pos] = Math.Cos(2 * Math.PI * i / _fftSize);
                _sinTbl[pos] = Math.Sin(2 * Math.PI * i / _fftSize);
            }
        }

        /// <summary>
        /// Do Fast Fourier Transform: 
        /// complex (<paramref name="reInput"/>, <paramref name="imInput"/>) -> complex(<paramref name="re"/>, <paramref name="im"/>).
        /// </summary>
        /// <param name="reInput">Array of real parts (input)</param>
        /// <param name="imInput">Array of imaginary parts (input)</param>
        /// <param name="re">Array of real parts (output)</param>
        /// <param name="im">Array of imaginary parts (output)</param>
        public void Direct(double[] reInput, double[] imInput, double[] re, double[] im)
        {
            reInput.FastCopyTo(re, reInput.Length);
            imInput.FastCopyTo(im, imInput.Length);

            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
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
        /// Do Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Direct(double[] re, double[] im)
        {
            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
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
        /// Do Inverse Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Inverse(double[] re, double[] im)
        {
            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
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
        /// Do Inverse Fast Fourier Transform in-place, with normalization by FFT size.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void InverseNorm(double[] re, double[] im)
        {
            Inverse(re, im);

            for (int i = 0; i < _fftSize; i++)
            {
                re[i] /= _fftSize;
                im[i] /= _fftSize;
            }
        }

#if NET50
        /// <summary>
        /// Do Fast Fourier Transform: 
        /// complex (<paramref name="reInput"/>, <paramref name="imInput"/>) -> complex(<paramref name="re"/>, <paramref name="im"/>).
        /// </summary>
        /// <param name="reInput">Array of real parts (input)</param>
        /// <param name="imInput">Array of imaginary parts (input)</param>
        /// <param name="re">Array of real parts (output)</param>
        /// <param name="im">Array of imaginary parts (output)</param>
        public void Direct(ReadOnlySpan<double> reInput, ReadOnlySpan<double> imInput, Span<double> re, Span<double> im)
        {
            reInput.CopyTo(re);
            imInput.CopyTo(im);

            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
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
        /// Do Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Direct(Span<double> re, Span<double> im)
        {
            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
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
        /// Do Inverse Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Inverse(Span<double> re, Span<double> im)
        {
            var L = _fftSize;
            var M = _fftSize >> 1;
            var S = _fftSize - 1;
            var ti = 0;
            while (L >= 2)
            {
                var l = L >> 1;
                var u1 = 1.0;
                var u2 = 0.0;
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
        /// Do Inverse Fast Fourier Transform in-place, with normalization by FFT size.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void InverseNorm(Span<double> re, Span<double> im)
        {
            Inverse(re, im);

            for (int i = 0; i < _fftSize; i++)
            {
                re[i] /= _fftSize;
                im[i] /= _fftSize;
            }
        }
#endif
    }
}
