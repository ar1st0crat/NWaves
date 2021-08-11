using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// FFT transformer for real inputs (double precision)
    /// </summary>
    public class RealFft64
    {
        /// <summary>
        /// Size of FFT
        /// </summary>
        public int Size => _fftSize * 2;

        /// <summary>
        /// Half of FFT size (for calculations)
        /// </summary>
        private int _fftSize;

        /// <summary>
        /// Precomputed cosines
        /// </summary>
        private readonly double[] _cosTbl;

        /// <summary>
        /// Precomputed sines
        /// </summary>
        private readonly double[] _sinTbl;

        /// <summary>
        /// Precomputed coefficients
        /// </summary>
        private double[] _ar, _br, _ai, _bi;

        /// <summary>
        /// Internal buffers
        /// </summary>
        private readonly double[] _re;
        private readonly double[] _im;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"></param>
        public RealFft64(int size)
        {
            Guard.AgainstNotPowerOfTwo(size, "Size of FFT");

            _fftSize = size / 2;

            _re = new double[_fftSize];
            _im = new double[_fftSize];

            // precompute coefficients:

            var tblSize = (int)Math.Log(_fftSize, 2);

            _cosTbl = new double[tblSize];
            _sinTbl = new double[tblSize];

            for (int i = 1, pos = 0; i < _fftSize; i *= 2, pos++)
            {
                _cosTbl[pos] = Math.Cos(2 * Math.PI * i / _fftSize);
                _sinTbl[pos] = Math.Sin(2 * Math.PI * i / _fftSize);
            }

            _ar = new double[_fftSize];
            _br = new double[_fftSize];
            _ai = new double[_fftSize];
            _bi = new double[_fftSize];

            var f = Math.PI / _fftSize;

            for (var i = 0; i < _fftSize; i++)
            {
                _ar[i] =  0.5 * (1 - Math.Sin(f * i));
                _ai[i] = -0.5 * Math.Cos(f * i);
                _br[i] =  0.5 * (1 + Math.Sin(f * i));
                _bi[i] =  0.5 * Math.Cos(f * i);
            }
        }

        /// <summary>
        /// Direct transform
        /// </summary>
        /// <param name="input"></param>
        /// <param name="re"></param>
        /// <param name="im"></param>
        public void Direct(double[] input, double[] re, double[] im)
        {
            // do half-size complex FFT:

            for (int i = 0, k = 0; i < _fftSize; i++)
            {
                _re[i] = input[k++];
                _im[i] = input[k++];
            }

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
                        var t1 = _re[i] + _re[p];
                        var t2 = _im[i] + _im[p];
                        var t3 = _re[i] - _re[p];
                        var t4 = _im[i] - _im[p];
                        _re[p] = t3 * u1 - t4 * u2;
                        _im[p] = t4 * u1 + t3 * u2;
                        _re[i] = t1;
                        _im[i] = t2;
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
                    var t1 = _re[j];
                    var t2 = _im[j];
                    _re[j] = _re[i];
                    _im[j] = _im[i];
                    _re[i] = t1;
                    _im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // do the last step:

            re[0] = _re[0] * _ar[0] - _im[0] * _ai[0] + _re[0] * _br[0] + _im[0] * _bi[0];
            im[0] = _im[0] * _ar[0] + _re[0] * _ai[0] + _re[0] * _bi[0] - _im[0] * _br[0];

            for (var k = 1; k < _fftSize; k++)
            {
                re[k] = _re[k] * _ar[k] - _im[k] * _ai[k] + _re[_fftSize - k] * _br[k] + _im[_fftSize - k] * _bi[k];
                im[k] = _im[k] * _ar[k] + _re[k] * _ai[k] + _re[_fftSize - k] * _bi[k] - _im[_fftSize - k] * _br[k];
            }

            re[_fftSize] = _re[0] - _im[0];
            im[_fftSize] = 0;
        }

        /// <summary>
        /// Inverse transform
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <param name="output"></param>
        public void Inverse(double[] re, double[] im, double[] output)
        {
            // do the first step:

            for (var k = 0; k < _fftSize; k++)
            {
                _re[k] = re[k] * _ar[k] + im[k] * _ai[k] + re[_fftSize - k] * _br[k] - im[_fftSize - k] * _bi[k];
                _im[k] = im[k] * _ar[k] - re[k] * _ai[k] - re[_fftSize - k] * _bi[k] - im[_fftSize - k] * _br[k];
            }

            // do half-size complex FFT:

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
                        var t1 = _re[i] + _re[p];
                        var t2 = _im[i] + _im[p];
                        var t3 = _re[i] - _re[p];
                        var t4 = _im[i] - _im[p];
                        _re[p] = t3 * u1 - t4 * u2;
                        _im[p] = t4 * u1 + t3 * u2;
                        _re[i] = t1;
                        _im[i] = t2;
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
                    var t1 = _re[j];
                    var t2 = _im[j];
                    _re[j] = _re[i];
                    _im[j] = _im[i];
                    _re[i] = t1;
                    _im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // fill output:

            for (int i = 0, k = 0; i < _fftSize; i++)
            {
                output[k++] = _re[i] * 2;
                output[k++] = _im[i] * 2;
            }
        }

        /// <summary>
        /// Inverse transform
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <param name="output"></param>
        public void InverseNorm(double[] re, double[] im, double[] output)
        {
            // do the first step:

            for (var k = 0; k < _fftSize; k++)
            {
                _re[k] = re[k] * _ar[k] + im[k] * _ai[k] + re[_fftSize - k] * _br[k] - im[_fftSize - k] * _bi[k];
                _im[k] = im[k] * _ar[k] - re[k] * _ai[k] - re[_fftSize - k] * _bi[k] - im[_fftSize - k] * _br[k];
            }

            // do half-size complex FFT:

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
                        var t1 = _re[i] + _re[p];
                        var t2 = _im[i] + _im[p];
                        var t3 = _re[i] - _re[p];
                        var t4 = _im[i] - _im[p];
                        _re[p] = t3 * u1 - t4 * u2;
                        _im[p] = t4 * u1 + t3 * u2;
                        _re[i] = t1;
                        _im[i] = t2;
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
                    var t1 = _re[j];
                    var t2 = _im[j];
                    _re[j] = _re[i];
                    _im[j] = _im[i];
                    _re[i] = t1;
                    _im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // fill output:

            for (int i = 0, k = 0; i < _fftSize; i++)
            {
                output[k++] = _re[i] / _fftSize;
                output[k++] = _im[i] / _fftSize;
            }
        }

#if NET50
        /// <summary>
        /// Direct transform
        /// </summary>
        /// <param name="input"></param>
        /// <param name="re"></param>
        /// <param name="im"></param>
        public void Direct(ReadOnlySpan<double> input, Span<double> re, Span<double> im)
        {
            // do half-size complex FFT:

            for (int i = 0, k = 0; i < _fftSize; i++)
            {
                _re[i] = input[k++];
                _im[i] = input[k++];
            }

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
                        var t1 = _re[i] + _re[p];
                        var t2 = _im[i] + _im[p];
                        var t3 = _re[i] - _re[p];
                        var t4 = _im[i] - _im[p];
                        _re[p] = t3 * u1 - t4 * u2;
                        _im[p] = t4 * u1 + t3 * u2;
                        _re[i] = t1;
                        _im[i] = t2;
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
                    var t1 = _re[j];
                    var t2 = _im[j];
                    _re[j] = _re[i];
                    _im[j] = _im[i];
                    _re[i] = t1;
                    _im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // do the last step:

            re[0] = _re[0] * _ar[0] - _im[0] * _ai[0] + _re[0] * _br[0] + _im[0] * _bi[0];
            im[0] = _im[0] * _ar[0] + _re[0] * _ai[0] + _re[0] * _bi[0] - _im[0] * _br[0];

            for (var k = 1; k < _fftSize; k++)
            {
                re[k] = _re[k] * _ar[k] - _im[k] * _ai[k] + _re[_fftSize - k] * _br[k] + _im[_fftSize - k] * _bi[k];
                im[k] = _im[k] * _ar[k] + _re[k] * _ai[k] + _re[_fftSize - k] * _bi[k] - _im[_fftSize - k] * _br[k];
            }

            re[_fftSize] = _re[0] - _im[0];
            im[_fftSize] = 0;
        }

        /// <summary>
        /// Inverse transform
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <param name="output"></param>
        public void Inverse(ReadOnlySpan<double> re, ReadOnlySpan<double> im, Span<double> output)
        {
            // do the first step:

            for (var k = 0; k < _fftSize; k++)
            {
                _re[k] = re[k] * _ar[k] + im[k] * _ai[k] + re[_fftSize - k] * _br[k] - im[_fftSize - k] * _bi[k];
                _im[k] = im[k] * _ar[k] - re[k] * _ai[k] - re[_fftSize - k] * _bi[k] - im[_fftSize - k] * _br[k];
            }

            // do half-size complex FFT:

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
                        var t1 = _re[i] + _re[p];
                        var t2 = _im[i] + _im[p];
                        var t3 = _re[i] - _re[p];
                        var t4 = _im[i] - _im[p];
                        _re[p] = t3 * u1 - t4 * u2;
                        _im[p] = t4 * u1 + t3 * u2;
                        _re[i] = t1;
                        _im[i] = t2;
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
                    var t1 = _re[j];
                    var t2 = _im[j];
                    _re[j] = _re[i];
                    _im[j] = _im[i];
                    _re[i] = t1;
                    _im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // fill output:

            for (int i = 0, k = 0; i < _fftSize; i++)
            {
                output[k++] = _re[i] * 2;
                output[k++] = _im[i] * 2;
            }
        }

        /// <summary>
        /// Inverse transform (with normalization by Fft size)
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <param name="output"></param>
        public void InverseNorm(ReadOnlySpan<double> re, ReadOnlySpan<double> im, Span<double> output)
        {
            // do the first step:

            for (var k = 0; k < _fftSize; k++)
            {
                _re[k] = re[k] * _ar[k] + im[k] * _ai[k] + re[_fftSize - k] * _br[k] - im[_fftSize - k] * _bi[k];
                _im[k] = im[k] * _ar[k] - re[k] * _ai[k] - re[_fftSize - k] * _bi[k] - im[_fftSize - k] * _br[k];
            }

            // do half-size complex FFT:

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
                        var t1 = _re[i] + _re[p];
                        var t2 = _im[i] + _im[p];
                        var t3 = _re[i] - _re[p];
                        var t4 = _im[i] - _im[p];
                        _re[p] = t3 * u1 - t4 * u2;
                        _im[p] = t4 * u1 + t3 * u2;
                        _re[i] = t1;
                        _im[i] = t2;
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
                    var t1 = _re[j];
                    var t2 = _im[j];
                    _re[j] = _re[i];
                    _im[j] = _im[i];
                    _re[i] = t1;
                    _im[i] = t2;
                }
                var k = M;
                while (j >= k)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // fill output:

            for (int i = 0, k = 0; i < _fftSize; i++)
            {
                output[k++] = _re[i] / _fftSize;
                output[k++] = _im[i] / _fftSize;
            }
        }
#endif
    }
}
