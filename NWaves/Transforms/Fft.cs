using System;

namespace NWaves.Transforms
{
    public static partial class Transform
    {
        /// <summary>
        /// Fast Fourier Transform algorithm
        /// </summary>
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        /// <param name="n">FFT size (must be the power of two)</param>
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
        /// <param name="re">Array of real parts</param>
        /// <param name="im">Array of imaginary parts</param>
        /// <param name="n">FFT size (must be the power of two)</param>
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
    }
}
