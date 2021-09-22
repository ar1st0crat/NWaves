using NWaves.Signals;
using NWaves.Transforms.Base;
using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// <para>Class representing Complex Fast Fourier Transform:</para>
    /// <list type="bullet">
    ///     <item>Direct FFT</item>
    ///     <item>Inverse FFT</item>
    ///     <item>Magnitude spectrum</item>
    ///     <item>Power spectrum</item>
    /// </list>
    /// </summary>
    public class Fft : IComplexTransform
    {
        /// <summary>
        /// FFT size.
        /// </summary>
        public int Size => _fftSize;
        private readonly int _fftSize;

        /// <summary>
        /// Precomputed cosines.
        /// </summary>
        private readonly float[] _cosTbl;

        /// <summary>
        /// Precomputed sines.
        /// </summary>
        private readonly float[] _sinTbl;

        /// <summary>
        /// Intermediate buffer storing real parts of spectrum.
        /// </summary>
        private readonly float[] _realSpectrum;

        /// <summary>
        /// Intermediate buffer storing imaginary parts of spectrum.
        /// </summary>
        private readonly float[] _imagSpectrum;

        /// <summary>
        /// Construct FFT transformer with given <paramref name="fftSize"/>. FFT size must be a power of two.
        /// </summary>
        /// <param name="fftSize">FFT size</param>
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
        /// Do Fast Fourier Transform in-place.
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
        /// Do Inverse Fast Fourier Transform in-place.
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
        /// Do normalized Inverse Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void InverseNorm(float[] re, float[] im)
        {
            Inverse(re, im);

            for (int i = 0; i < _fftSize; i++)
            {
                re[i] /= _fftSize;
                im[i] /= _fftSize;
            }
        }

        /// <summary>
        /// Do Fast Fourier Transform: 
        /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void Direct(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            inRe.FastCopyTo(outRe, inRe.Length);
            inIm.FastCopyTo(outIm, inIm.Length);

            Direct(outRe, outIm);
        }

        /// <summary>
        /// Do normalized Fast Fourier Transform: 
        /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void DirectNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            Direct(inRe, inIm, outRe, outIm);
        }

        /// <summary>
        /// Do Inverse Fast Fourier Transform: 
        /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void Inverse(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            inRe.FastCopyTo(outRe, inRe.Length);
            inIm.FastCopyTo(outIm, inIm.Length);

            Inverse(outRe, outIm);
        }

        /// <summary>
        /// Do normalized Inverse Fast Fourier Transform: 
        /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void InverseNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            inRe.FastCopyTo(outRe, inRe.Length);
            inIm.FastCopyTo(outIm, inIm.Length);

            InverseNorm(outRe, outIm);
        }

        /// <summary>
        /// <para>Compute magnitude spectrum from <paramref name="samples"/>:</para>
        /// <code>
        ///     spectrum = sqrt(re * re + im * im)
        /// </code>
        /// <para>Method fills array <paramref name="spectrum"/>. It must have size at least fftSize/2+1.</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <param name="normalize">Normalize by FFT size or not</param>
        public void MagnitudeSpectrum(float[] samples, float[] spectrum, bool normalize = false)
        {
            Array.Clear(_realSpectrum, 0, _fftSize);
            Array.Clear(_imagSpectrum, 0, _fftSize);

            samples.FastCopyTo(_realSpectrum, Math.Min(samples.Length, _fftSize));

            Direct(_realSpectrum, _imagSpectrum);

            var n = _fftSize / 2;

            if (normalize)
            {
                spectrum[0] = Math.Abs(_realSpectrum[0]) / _fftSize;
                spectrum[n] = Math.Abs(_realSpectrum[n]) / _fftSize;

                for (var i = 1; i < n; i++)
                {
                    spectrum[i] = (float)(Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize);
                }
            }
            else
            {
                spectrum[0] = Math.Abs(_realSpectrum[0]);
                spectrum[n] = Math.Abs(_realSpectrum[n]);

                for (var i = 1; i < n; i++)
                {
                    spectrum[i] = (float)(Math.Sqrt(_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]));
                }
            }
        }

        /// <summary>
        /// <para>Compute power spectrum from <paramref name="samples"/>:</para>
        /// <code>
        ///     spectrum = (re * re + im * im)
        /// </code>
        /// <para>Method fills array <paramref name="spectrum"/>. It must have size at least fftSize/2+1.</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <param name="normalize">Normalize by FFT size or not</param>
        public void PowerSpectrum(float[] samples, float[] spectrum, bool normalize = true)
        {
            Array.Clear(_realSpectrum, 0, _fftSize);
            Array.Clear(_imagSpectrum, 0, _fftSize);

            samples.FastCopyTo(_realSpectrum, Math.Min(samples.Length, _fftSize));

            Direct(_realSpectrum, _imagSpectrum);

            var n = _fftSize / 2; 

            if (normalize)
            {
                spectrum[0] = _realSpectrum[0] * _realSpectrum[0] / _fftSize;
                spectrum[n] = _realSpectrum[n] * _realSpectrum[n] / _fftSize;

                for (var i = 1; i < n; i++)
                {
                    spectrum[i] = (_realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i]) / _fftSize;
                }
            }
            else
            {
                spectrum[0] = _realSpectrum[0] * _realSpectrum[0];
                spectrum[n] = _realSpectrum[n] * _realSpectrum[n];

                for (var i = 1; i < n; i++)
                {
                    spectrum[i] = _realSpectrum[i] * _realSpectrum[i] + _imagSpectrum[i] * _imagSpectrum[i];
                }
            }
        }

        /// <summary>
        /// <para>Compute and return magnitude spectrum from <paramref name="signal"/>:</para>
        /// <code>
        ///     spectrum = sqrt(re * re + im * im)
        /// </code>
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="normalize">Normalize by FFT size or not</param>
        public DiscreteSignal MagnitudeSpectrum(DiscreteSignal signal, bool normalize = false)
        {
            var spectrum = new float[_fftSize / 2 + 1];
            MagnitudeSpectrum(signal.Samples, spectrum, normalize);
            return new DiscreteSignal(signal.SamplingRate, spectrum);
        }

        /// <summary>
        /// <para>Compute and return power spectrum from <paramref name="signal"/>:</para>
        /// <code>
        ///     spectrum = (re * re + im * im)
        /// </code>
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="normalize">Normalize by FFT size or not</param>
        public DiscreteSignal PowerSpectrum(DiscreteSignal signal, bool normalize = true)
        {
            var spectrum = new float[_fftSize / 2 + 1];
            PowerSpectrum(signal.Samples, spectrum, normalize);
            return new DiscreteSignal(signal.SamplingRate, spectrum);
        }

        /// <summary>
        /// FFT shift in-place. Throws <see cref="ArgumentException"/> if array of <paramref name="samples"/> has odd length.
        /// </summary>
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


#if NET50
        /// <summary>
        /// Do Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Direct(Span<float> re, Span<float> im)
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
        /// Do Inverse Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void Inverse(Span<float> re, Span<float> im)
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
        /// Do normalized Inverse Fast Fourier Transform in-place.
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        public void InverseNorm(Span<float> re, Span<float> im)
        {
            Inverse(re, im);

            for (int i = 0; i < _fftSize; i++)
            {
                re[i] /= _fftSize;
                im[i] /= _fftSize;
            }
        }

        /// <summary>
        /// Do Fast Fourier Transform: 
        /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
        /// </summary>
        /// <param name="inRe">Array of real parts (input)</param>
        /// <param name="inIm">Array of imaginary parts (input)</param>
        /// <param name="outRe">Array of real parts (output)</param>
        /// <param name="outIm">Array of imaginary parts (output)</param>
        public void Direct(ReadOnlySpan<float> inRe, ReadOnlySpan<float> inIm, Span<float> outRe, Span<float> outIm)
        {
            inRe.CopyTo(outRe);
            inIm.CopyTo(outIm);

            Direct(outRe, outIm);
        }

        /// <summary>
        /// Do Inverse Fast Fourier Transform: 
        /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
        /// </summary>
        /// <param name="inRe">Array of real parts (input)</param>
        /// <param name="inIm">Array of imaginary parts (input)</param>
        /// <param name="outRe">Array of real parts (output)</param>
        /// <param name="outIm">Array of imaginary parts (output)</param>
        public void Inverse(ReadOnlySpan<float> inRe, ReadOnlySpan<float> inIm, Span<float> outRe, Span<float> outIm)
        {
            inRe.CopyTo(outRe);
            inIm.CopyTo(outIm);

            Inverse(outRe, outIm);
        }

        /// <summary>
        /// Do normalized Inverse Fast Fourier Transform: 
        /// complex (<paramref name="inRe"/>, <paramref name="inIm"/>) -> complex(<paramref name="outRe"/>, <paramref name="outIm"/>).
        /// </summary>
        /// <param name="inRe">Array of real parts (input)</param>
        /// <param name="inIm">Array of imaginary parts (input)</param>
        /// <param name="outRe">Array of real parts (output)</param>
        /// <param name="outIm">Array of imaginary parts (output)</param>
        public void InverseNorm(ReadOnlySpan<float> inRe, ReadOnlySpan<float> inIm, Span<float> outRe, Span<float> outIm)
        {
            inRe.CopyTo(outRe);
            inIm.CopyTo(outIm);

            InverseNorm(outRe, outIm);
        }
#endif
    }
}
