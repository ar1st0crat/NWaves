using System;
using System.Numerics;

namespace NWaves.Utils
{
    /// <summary>
    /// Static class providing some helpful math functions
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Sinc-function
        /// </summary>
        /// <param name="x">Argument</param>
        /// <returns>sinc(x)</returns>
        public static double Sinc(double x)
        {
            return Math.Abs(x) > 1e-20 ? Math.Sin(Math.PI * x) / (Math.PI * x) : 1.0;
        }

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
        /// Modulo function that works correctly with negative numbers (as np.mod)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Mod(double a, double b)
        {
            return ((a % b) + b) % b;
        }

        /// <summary>
        /// Linear interpolation (as numpy.interp)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static float[] InterpolateLinear(float[] x, float[] y, float[] arg)
        {
            var interp = new float[arg.Length];

            var left = 0;
            var right = 1;

            for (var i = 0; i < arg.Length; i++)
            {
                while (arg[i] > x[right] && right < x.Length - 1)
                {
                    right++;
                    left++;
                }

                interp[i] = y[left] + (y[right] - y[left]) * (arg[i] - x[left]) / (x[right] - x[left]);
            }

            return interp;
        }

        /// <summary>
        /// Phase unwrap
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="tolerance">Jump size</param>
        /// <returns></returns>
        public static double[] Unwrap(double[] phase, double tolerance = Math.PI)
        {
            var unwrapped = phase.FastCopy();

            for (var n = 1; n < phase.Length; n++)
            {
                var delta = phase[n] - phase[n - 1];

                if (delta > tolerance)
                {
                    for (var i = n; i < unwrapped.Length; i++)
                    {
                        unwrapped[i] -= tolerance * 2;
                    }
                }
                else if (delta < -tolerance)
                {
                    for (var i = n; i < unwrapped.Length; i++)
                    {
                        unwrapped[i] += tolerance * 2;
                    }
                }
            }

            return unwrapped;
        }

        /// <summary>
        /// Levinson-Durbin algorithm for solving main LPC task
        /// </summary>
        /// <param name="input">Auto-correlation vector</param>
        /// <param name="a">LP coefficients</param>
        /// <param name="order">Order of LPC</param>
        /// <returns>Prediction error</returns>
        public static float LevinsonDurbin(float[] input, float[] a, int order)
        {
            var err = input[0];

            a[0] = 1.0f;

            for (var i = 1; i <= order; i++)
            {
                var lambda = 0.0f;
                for (var j = 0; j < i; j++)
                {
                    lambda -= a[j] * input[i - j];
                }

                lambda /= err;

                for (var n = 0; n <= i / 2; n++)
                {
                    var tmp = a[i - n] + lambda * a[n];
                    a[n] = a[n] + lambda * a[i - n];
                    a[i - n] = tmp;
                }

                err *= (1.0f - lambda * lambda);
            }

            return err;
        }

        /// <summary>
        /// Method implementing Durand-Kerner algorithm for finding complex roots of polynomials.
        /// Works for polynomials of order up to approx. 45. 
        /// </summary>
        /// <param name="a">Polynomial coefficients</param>
        /// <returns></returns>
        public static Complex[] PolynomialRoots(double[] a)
        {
            var n = a.Length;
            if (n <= 1)
            {
                return null;
            }

            const int maxIterations = 10000;

            var rootsPrev = new Complex[a.Length - 1];
            var roots = new Complex[a.Length - 1];

            var result = new Complex(0.4, 0.9);
            rootsPrev[0] = Complex.One;

            for (var i = 1; i < rootsPrev.Length; i++)
            {
                rootsPrev[i] = rootsPrev[i - 1] * result;
            }

            var iter = 0;
            while (true)
            {
                for (int i = 0; i < rootsPrev.Length; i++)
                {
                    result = Complex.One;

                    for (int j = 0; j < rootsPrev.Length; j++)
                    {
                        if (i != j)
                        {
                            result = (rootsPrev[i] - rootsPrev[j]) * result;
                        }
                    }

                    roots[i] = rootsPrev[i] - (EvaluatePolynomial(a, rootsPrev[i]) / result);
                }

                if (++iter > maxIterations || ArraysAreEqual(rootsPrev, roots))
                {
                    break;
                }

                Array.Copy(roots, rootsPrev, roots.Length);
            }

            return roots;
        }

        /// <summary>
        /// Method checks if two arrays of complex numbers are essentially identical
        /// </summary>
        /// <param name="a">First array</param>
        /// <param name="b">Second array</param>
        /// <param name="tolerance">Tolerance level</param>
        /// <returns>true if arrays are equal</returns>
        private static bool ArraysAreEqual(Complex[] a, Complex[] b, double tolerance = 1e-14)
        {
            for (var i = 0; i < a.Length; i++)
            {
                var delta = a[i] - b[i];

                if (Math.Abs(delta.Real) > tolerance || Math.Abs(delta.Imaginary) > tolerance)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Evaluate polynomial according to Horner scheme
        /// </summary>
        /// <param name="a">Polynomial coefficients</param>
        /// <param name="x">x</param>
        /// <returns>The value of polynomial</returns>
        public static Complex EvaluatePolynomial(double[] a, Complex x)
        {
            var res = new Complex(a[0], 0);

            for (var i = 1; i < a.Length; i++)
            {
                res *= x;
                res += a[i];
            }

            return res;
        }
    }
}
