using System;
using System.Linq;

namespace NWaves.Transforms
{
    public static partial class Transform
    {
        /// <summary>
        /// Fast Fourier Transform algorithm
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <param name="n"></param>
        public static void Fft(double[] re, double[] im, int n)
        {
            double t1, t2;
            int i, j;
            int L, M, S;

            L = n;
            M = n >> 1;
            S = n - 1;
            while (L >= 2)
            {
                var l = L >> 1;
                t1 = Math.PI / l;
                var u1 = 1.0;
                var u2 = 0.0;
                var c = Math.Cos(t1);
                var s = -Math.Sin(t1);
                for (j = 0; j < l; j++)
                {
                    for (i = j; i < n; i += L)
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
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <param name="n"></param>
        public static void Ifft(double[] re, double[] im, int n)
        {
            double t1, t2;
            int i, j;
            int L, M, S;

            L = n;
            M = n >> 1;
            S = n - 1;
            while (L >= 2)
            {
                var l = L >> 1;
                t1 = Math.PI / l;
                var u1 = 1.0;
                var u2 = 0.0;
                var c = Math.Cos(t1);
                var s = Math.Sin(t1);
                for (j = 0; j < l; j++)
                {
                    for (i = j; i < n; i += L)
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
        /// Magnitude spectrum:
        /// 
        ///     spectrum = sqrt(re * re + im * im)
        /// 
        /// </summary>
        /// <param name="real">Array of samples (real parts)</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <returns>Left HALF of the magnitude spectrum</returns>
        /// 
        /// NOTE: method expects FFT size to be a power of 2
        ///       however, for the sake of performance, does NOT check FFT size
        /// 
        public static double[] MagnitudeSpectrum(double[] real, int fftSize = 512)
        {
            var imag = new double[real.Length];

            Fft(real, imag, fftSize);

            var reals = real.Take(fftSize / 2);
            var imags = imag.Take(fftSize / 2);

            return reals.Zip(imags, (re, im) => Math.Sqrt(re * re + im * im)).ToArray();
        }

        /// <summary>
        /// Log power spectrum:
        /// 
        ///     spectrum = 20 * log10(re * re + im * im)
        /// 
        /// </summary>
        /// <param name="real">Array of samples (real parts)</param>
        /// <param name="fftSize">Size of FFT</param>
        /// 
        /// NOTE: method expects FFT size to be a power of 2
        ///       however, for the sake of performance, does NOT check FFT size
        /// 
        /// <returns>Left HALF of the log-power spectrum</returns>
        public static double[] LogPowerSpectrum(double[] real, int fftSize = 512)
        {
            var imag = new double[real.Length];

            Fft(real, imag, fftSize);

            var reals = real.Take(fftSize / 2);
            var imags = imag.Take(fftSize / 2);

            return reals.Zip(imags, (re, im) => 20 * Math.Log10(re * re + im * im + double.MinValue)).ToArray();
        }
    }
}
