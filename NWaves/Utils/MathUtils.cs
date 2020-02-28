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
        /// Inverse sinh
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Asinh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x + 1));
        }

        /// <summary>
        /// Factorial
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double Factorial(int n)
        {
            var f = 1.0;

            for (var i = 2; i <= n; f *= i++) ;

            return f;
        }

        /// <summary>
        /// Binomial coefficient
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static double BinomialCoefficient(int k, int n)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        /// <summary>
        /// Diff signal (1st order derivative)
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="diff"></param>
        public static void Diff(float[] samples, float[] diff)
        {
            diff[0] = samples[0];

            for (var i = 1; i < samples.Length; i++)
            {
                diff[i] = samples[i] - samples[i - 1];
            }
        }

        /// <summary>
        /// Linear interpolation (as numpy.interp)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="arg"></param>
        /// <param name="interp"></param>
        /// <returns></returns>
        public static void InterpolateLinear(float[] x, float[] y, float[] arg, float[] interp)
        {
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
        }

        /// <summary>
        /// Bilinear transform (in-place)
        /// </summary>
        /// <param name="re"></param>
        /// <param name="im"></param>
        public static void BilinearTransform(double[] re, double[] im)
        {
            for (var k = 0; k < re.Length; k++)
            {
                var den = (1 - re[k]) * (1 - re[k]) + im[k] * im[k];
                re[k] = (1 - re[k] * re[k] - im[k] * im[k]) / den;
                im[k] = 2 * im[k] / den;
            }

            // equivalent to:

            //for (var k = 0; k < re.Length; k++)
            //{
            //      var c1 = new Complex(1 + re[k],  im[k]);
            //      var c2 = new Complex(1 - re[k], -im[k]);
            //      var c = c1 / c2;

            //      re[k] = c.Real;
            //      im[k] = c.Imaginary;
            //}
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
        /// Modified Bessel function of the 1st kind (Taylor series, not very precise method)
        /// </summary>
        /// <param name="x">x</param>
        /// <returns>I0(x)</returns>
        public static double I0(double x)
        {
            double y = 1.0;
            double prev = 1.0;
            double summand = 0;

            var i = 1;

            while (Math.Abs(prev) > 1e-20)
            {
                summand = prev * x * x / (4 * i * i);
                y += summand;
                prev = summand;
                i++;
            }

            return y;
        }


        #region polynomials

        public const int PolyRootsIterations = 25000;

        /// <summary>
        /// Method implementing Durand-Kerner algorithm for finding complex roots of polynomials.
        /// Works for polynomials of order up to approx. 50. 
        /// </summary>
        /// <param name="a">Polynomial coefficients</param>
        /// <returns></returns>
        public static Complex[] PolynomialRoots(double[] a, int maxIterations = PolyRootsIterations)
        {
            var n = a.Length;
            if (n <= 1)
            {
                return null;
            }

            var c1 = Complex.One;

            var rootsPrev = new Complex[a.Length - 1];
            var roots = new Complex[a.Length - 1];

            var result = new Complex(0.4, 0.9);
            rootsPrev[0] = c1;

            for (var i = 1; i < rootsPrev.Length; i++)
            {
                rootsPrev[i] = rootsPrev[i - 1] * result;
            }

            var iter = 0;
            while (true)
            {
                for (int i = 0; i < rootsPrev.Length; i++)
                {
                    result = c1;

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
        private static bool ArraysAreEqual(Complex[] a, Complex[] b, double tolerance = 1e-16)
        {
            for (var i = 0; i < a.Length; i++)
            {
                if (Complex.Abs(a[i] - b[i]) > tolerance)
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
        /// Multiply polynomials
        /// </summary>
        /// <param name="poly1"></param>
        /// <param name="poly2"></param>
        /// <returns></returns>
        public static Complex[] MultiplyPolynomials(Complex[] poly1, Complex[] poly2)
        {
            var length = poly1.Length + poly2.Length - 1;
            var result = new Complex[length];

            for (var i = 0; i < poly1.Length; i++)
            {
                for (var j = 0; j < poly2.Length; j++)
                {
                    result[i + j] += poly1[i] * poly2[j];
                }
            }

            return result;
        }

        /// <summary>
        /// Divide polynomials
        /// </summary>
        /// <param name="dividend">Dividend</param>
        /// <param name="divisor">Divisor</param>
        /// <returns></returns>
        public static Complex[][] DividePolynomial(Complex[] dividend, Complex[] divisor)
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

        #endregion
    }
}
