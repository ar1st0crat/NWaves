﻿using NWaves.Utils;
using System;
using System.Numerics;

namespace NWaves.Filters.Elliptic
{
    // Orfanidis, S. J. (2007). Lecture notes on elliptic filter design.
    // URL: http://www.ece.rutgers.edu/~orfanidi/ece521/notes.pdf

    /// <summary>
    /// Elliptic filter prototype.
    /// </summary>
    public static class PrototypeElliptic
    {
        /// <summary>
        /// Evaluates analog poles of elliptic filter of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public static Complex[] Poles(int order, double ripplePass = 1, double rippleStop = 20)
        {
            Guard.AgainstInvalidRange(ripplePass, rippleStop, "ripple in passband", "ripple in stopband");

            var eps_p = Math.Sqrt(Math.Pow(10, ripplePass / 10) - 1);
            var eps_s = Math.Sqrt(Math.Pow(10, rippleStop / 10) - 1);

            var r = eps_p / eps_s;

            var k1 = Math.Sqrt(1 - r * r);
            var k1_landen = Landen(k1);

            var kp = Complex.One;
            for (var i = 0; i < order / 2; i++)
            {
                kp *= Sne((2 * i + 1.0) / order, k1_landen);
            }
            kp = Complex.Pow(k1 * k1, order / 2) * Complex.Pow(kp, 4);

            var k = Math.Sqrt(1 - Complex.Abs(kp) * Complex.Abs(kp));
            var k_landen = Landen(k);

            var v0 = -Complex.ImaginaryOne / order * Asne(Complex.ImaginaryOne / eps_p, r);

            var poles = new Complex[order];

            for (var i = 0; i < order; i++)
            {
                var w = (2 * i + 1.0) / order;

                poles[i] = Complex.ImaginaryOne * Cde(w - Complex.ImaginaryOne * v0, k_landen);
            }

            return poles;
        }

        /// <summary>
        /// Evaluates analog zeros of elliptic filter of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public static Complex[] Zeros(int order, double ripplePass = 1, double rippleStop = 20)
        {
            Guard.AgainstInvalidRange(ripplePass, rippleStop, "ripple in passband", "ripple in stopband");

            var eps_p = Math.Sqrt(Math.Pow(10, ripplePass / 10) - 1);
            var eps_s = Math.Sqrt(Math.Pow(10, rippleStop / 10) - 1);

            var r = eps_p / eps_s;

            var k1 = Math.Sqrt(1 - r * r);
            var k1_landen = Landen(k1);

            var kp = Complex.One;
            for (var i = 0; i < order / 2; i++)
            {
                kp *= Sne((2 * i + 1.0) / order, k1_landen);
            }
            kp = Complex.Pow(k1 * k1, order / 2) * Complex.Pow(kp, 4);

            var k = Math.Sqrt(1 - Complex.Abs(kp) * Complex.Abs(kp));
            var k_landen = Landen(k);

            var zeros = new Complex[order];

            for (var i = 0; i < order; i++)
            {
                var w = (2 * i + 1.0) / order;

                zeros[i] = new Complex(0, -1 / (k * Cde(w, k_landen)).Real);
            }

            return zeros;
        }

        /// <summary>
        /// Computes Landen sequence.
        /// </summary>
        /// <param name="k">K</param>
        /// <param name="iterCount">Number of iterations</param>
        public static double[] Landen(double k, int iterCount = 5)
        {
            var coeffs = new double[iterCount];

            for (var i = 0; i < iterCount; i++)
            {
                var kp = Math.Sqrt(1 - k * k);
                k = (1 - kp) / (1 + kp);
                coeffs[i] = k;
            }

            return coeffs;
        }

        /// <summary>
        /// Computes sde.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="landen">Landen sequence</param>
        public static Complex Cde(Complex x, double[] landen)
        {
            var invX = 1 / Complex.Cos(x * Math.PI / 2);

            for (var i = landen.Length - 1; i >= 0; i--)
            {
                invX = 1 / (1 + landen[i]) * (invX + landen[i] / invX);
            }

            return 1 / invX;
        }

        /// <summary>
        /// Computes sne.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="landen">Landen sequence</param>
        public static Complex Sne(Complex x, double[] landen)
        {
            var invX = 1 / Complex.Sin(x * Math.PI / 2);

            for (var i = landen.Length - 1; i >= 0; i--)
            {
                invX = 1 / (1 + landen[i]) * (invX + landen[i] / invX);
            }

            return 1 / invX;
        }

        /// <summary>
        /// Computes inverse sne.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="k">K</param>
        /// <param name="iterCount">Number of iterations</param>
        public static Complex Asne(Complex x, double k, int iterCount = 5)
        {
            for (var i = 1; i <= iterCount; i++)
            {
                var prevX = x;
                var prevK = k;

                k = Math.Pow(k / (1 + Math.Sqrt(1 - k * k)), 2);

                x = 2 * x / ((1 + k) * (1 + Complex.Sqrt(1 - prevK * prevK * x * x)));
            }

            return 2 * Complex.Asin(x) / Math.PI;
        }
    }
}
