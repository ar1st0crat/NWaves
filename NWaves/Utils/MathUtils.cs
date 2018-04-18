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
        /// Unwrap (phase)
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="tolerance">Jump size</param>
        /// <returns></returns>
        public static double[] Unwrap(double[] phase, double tolerance = Math.PI)
        {
            var unwrapped = phase.FastCopy();

            var offset = 0.0;

            for (var n = 1; n < phase.Length; n++)
            {
                var delta = phase[n] - phase[n - 1];

                if (delta > tolerance)
                {
                    offset -= tolerance * 2;
                }
                else if (delta < -tolerance)
                {
                    offset += tolerance * 2;
                }

                unwrapped[n] = phase[n] + offset;
            }

            return unwrapped;
        }

        /// <summary>
        /// Wrap (phase)
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="tolerance">Jump size</param>
        /// <returns></returns>
        public static double[] Wrap(double[] phase, double tolerance = Math.PI)
        {
            var wrapped = phase.FastCopy();

            for (var n = 0; n < phase.Length; n++)
            {
                var offset = phase[n] % (tolerance * 2);

                if (offset > tolerance)
                {
                    offset -= tolerance * 2;
                }
                else if (offset < -tolerance)
                {
                    offset += tolerance * 2;
                }

                wrapped[n] = offset;
            }

            return wrapped;
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
        /// Nth order statistics
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static float FindNth(float[] a, int n, int start, int end)
        {
            while (true)
            {
                // ============== Partitioning =============
                var pivotElem = a[end];
                var pivot = start - 1;
                for (var i = start; i < end; i++)
                {
                    if (a[i] <= pivotElem)
                    {
                        pivot++;
                        var temp = a[i];
                        a[i] = a[pivot];
                        a[pivot] = temp;
                    }
                }
                pivot++;
                var tmp = a[end];
                a[end] = a[pivot];
                a[pivot] = tmp;
                // ========================================
                
                if (pivot == n)
                {
                    return a[pivot];
                }
                if (n < pivot)
                {
                    end = pivot - 1;
                }
                else
                {
                    start = pivot + 1;
                }
            }
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

        /// <summary>
        /// Divide polynomials
        /// </summary>
        /// <param name="dividend">Dividend</param>
        /// <param name="divisor">Divisor</param>
        /// <returns></returns>
        public static Complex[][] PolynomialDivision(Complex[] dividend, Complex[] divisor)
        {
            var output = (Complex[])dividend.Clone();
            var normalizer = divisor[0];

            for (var i = 0; i < dividend.Length - divisor.Length + 1; i++)
            {
                output[i] /= normalizer;

                var coeff = output[i];
                if (Math.Abs(coeff.Real) > 1e-10 || Math.Abs(coeff.Imaginary) > 1e-10)
                {
                    for (var j = 1; j < divisor.Length; j++)
                    {
                        output[i + j] -= divisor[j] * coeff;
                    }
                }
            }

            var separator = output.Length - divisor.Length + 1;

            var q = new Complex[separator];
            var r = new Complex[output.Length - separator];

            Array.Copy(output, 0, q, 0, separator);
            Array.Copy(output, separator, r, 0, output.Length - separator);

            return new [] { q, r };
        }
    }
}
