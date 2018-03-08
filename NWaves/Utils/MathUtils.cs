using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Static class providing some helpful math functions
    /// </summary>
    static class MathUtils
    {
        /// <summary>
        /// Method for computing next power of 2 (closest to the given number)
        /// </summary>
        /// <param name="n">Number</param>
        /// <returns>Next power of 2 closest to the number</returns>
        public static int NextPowerOfTwo(int n)
        {
            return (int)Math.Pow(2, Math.Ceiling(Math.Log(n, 2)));
        }

        /// <summary>
        /// Greatest Common Divisor
        /// </summary>
        public static int Gcd(int n, int m)
        {
            while (m != 0)
            {
                m = n % (n = m);
            }
            return n;
        }

        /// <summary>
        /// Method implementing Durand-Kerner algorithm for finding complex roots of polynomials.
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        /// <returns></returns>
        public static Tuple<double[], double[]> PolynomialRoots(double[] re, double[] im)
        {
            var n = re.Length;
            if (n <= 1)
            {
                return null;
            }

            // Init

            var fftSize = NextPowerOfTwo(n);
            var pr = new double[fftSize];
            var pi = new double[fftSize];
        
            for (var i = 0; i < n; ++i)
            {
                pr[i] = re[i];
                pi[i] = im[i];
            }
            
            var ar = pr[n - 1];
            var br = pi[n - 1];
            var d = ar * ar + br * br;
            ar /= d;
            br /= -d;
            var s = br - ar;
            var t = ar + br;
            for (var i = 0; i < n - 1; ++i)
            {
                var t1 = ar * (pr[i] + pi[i]);
                var t2 = pr[i] * s;
                var t3 = pi[i] * t;
                pr[i] = t1 - t3;
                pi[i] = t1 + t2;
            }
            pr[n - 1] = 1.0;
            pi[n - 1] = 0.0;

            // Initial guess solution

            var zr = new double[n - 1];
            var zi = new double[n - 1];

            var bnd = 0.0;
            for (var i = 0; i < n; ++i)
            {
                bnd = Math.Max(bnd, pr[i] * pr[i] + pi[i] * pi[i]);
            }
            var r = 1.0 + Math.Sqrt(bnd);

            var rand = new Random();
            for (var i = 0; i < n - 1; ++i)
            {
                var ti = rand.NextDouble() * r;
                var ci = Math.Cos(rand.NextDouble() * 2 * Math.PI);
                zr[i] = ti * ci;
                zi[i] = ti * Math.Sqrt(1.0 - ci * ci);
            }


            // main routine

            var iters = 100 * n;

            const double tolerance = 1e-6;
            const double epsilon = 1e-8;

            for (var i = 0; i < iters; ++i)
            {
                var delta = 0.0;
                for (var j = 0; j < zr.Length; ++j)
                {
                    var pa = zr[j];
                    var pb = zi[j];

                    double qa, qb;

                    //Compute denominator
                    //
                    //  (zj - z0) * (zj - z1) * ... * (zj - z_{n-1})
                    //
                    var a = 1.0;
                    var b = 0.0;
                    for (var k = 0; k < zr.Length; ++k)
                    {
                        if (k == j)
                        {
                            continue;
                        }
                        qa = pa - zr[k];
                        qb = pb - zi[k];
                        if (qa * qa + qb * qb < tolerance)
                        {
                            continue;
                        }
                        var k1 = qa * (a + b);
                        var k2 = a * (qb - qa);
                        var k3 = b * (qa + qb);
                        a = k1 - k3;
                        b = k1 + k2;
                    }

                    //Compute numerator

                    var na = pr[n - 1];
                    var nb = pi[n - 1];
                    var s1 = pb - pa;
                    var s2 = pa + pb;
                    for (var k = n - 2; k >= 0; --k)
                    {
                        var k1 = pa * (na + nb);
                        var k2 = na * s1;
                        var k3 = nb * s2;
                        na = k1 - k3 + pr[k];
                        nb = k1 + k2 + pi[k];
                    }

                    //Compute reciprocal

                    var r1 = a * a + b * b;
                    if (Math.Abs(r1) > epsilon)
                    {
                        a /= r1;
                        b /= -r1;
                    }
                    else
                    {
                        a = 1.0;
                        b = 0.0;
                    }

                    //Multiply and accumulate

                    r1 = na * (a + b);
                    var r2 = a * (nb - na);
                    var r3 = b * (na + nb);

                    qa = r1 - r3;
                    qb = r1 + r2;

                    zr[j] = pa - qa;
                    zi[j] = pb - qb;

                    delta = Math.Max(d, Math.Max(Math.Abs(qa), Math.Abs(qb)));
                }

                if (delta < tolerance)
                {
                    break;
                }
            }

            // Combine repeated roots

            for (var i = 0; i < zr.Length; ++i)
            {
                var count = 1;
                var a = zr[i];
                var b = zi[i];
                for (var j = 0; j < zr.Length; ++j)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    if (AreCloseComplex(zr[i], zi[i], zr[j], zi[j], tolerance))
                    {
                        ++count;
                        a += zr[j];
                        b += zi[j];
                    }
                }
                if (count > 1)
                {
                    a /= count;
                    b /= count;
                    for (var j = 0; j < zr.Length; ++j)
                    {
                        if (i == j)
                        {
                            continue;
                        }
                        if (AreCloseComplex(zr[i], zi[i], zr[j], zi[j], tolerance))
                        {
                            zr[j] = a;
                            zi[j] = b;
                        }
                    }
                    zr[i] = a;
                    zi[i] = b;
                }
            }

            return new Tuple<double[], double[]>(zr, zi);
        }

        /// <summary>
        /// Method checks if two complex numbers are basically identical
        /// </summary>
        /// <param name="re1">Real part of the first number</param>
        /// <param name="im1">Imaginary part of the first number</param>
        /// <param name="re2">Real part of the second number</param>
        /// <param name="im2">Imaginary part of the second number</param>
        /// <param name="tolerance">The difference threshold indicating the numbers are basically identical</param>
        /// <returns></returns>
        public static bool AreCloseComplex(double re1, double im1, double re2, double im2, double tolerance)
        {
            var dre = re1 - re2;
            var dim = im1 - im2;
            var r = dre * dre + dim * dim;
            return r * r < tolerance;
        }
    }
}
