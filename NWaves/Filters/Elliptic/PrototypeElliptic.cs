using System;
using System.Numerics;

namespace NWaves.Filters.Elliptic
{
    public static class PrototypeElliptic
    {
        public static Complex[] Poles(int order, double ripplePass = 0.005, double rippleStop = 0.05)
        {
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

        public static Complex[] Zeros(int order, double ripplePass = 0.005, double rippleStop = 0.05)
        {
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

            //var k = Math.Sqrt(1 - Complex.Abs(kp * kp));
            var k = Math.Sqrt(1 - Complex.Abs(kp) * Complex.Abs(kp));
            var k_landen = Landen(k);

            var zeros = new Complex[order];

            for (var i = 0; i < order; i++)
            {
                var w = (2 * i + 1.0) / order;

                zeros[i] = -1 / (k * Cde(w, k_landen));
            }

            return zeros;
        }

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

        public static Complex Cde(Complex x, double[] landen)
        {
            var winv = 1 / Complex.Cos(x * Math.PI / 2);

            for (var i = landen.Length - 1; i >= 0; i--)
            {
                winv = 1 / (1 + landen[i]) * (winv + landen[i] / winv);
            }

            return 1 / winv;
        }

        public static Complex Sne(Complex x, double[] landen)
        {
            var winv = 1 / Complex.Sin(x * Math.PI / 2);

            for (var i = landen.Length - 1; i >= 0; i--)
            {
                winv = 1 / (1 + landen[i]) * (winv + landen[i] / winv);
            }

            return 1 / winv;
        }

        public static Complex Asne(Complex x, double k)
        {
            Complex xprev = double.NaN;

            while (Complex.Abs(x - xprev) > 1e-5)
            //for (var i = 1; i <= 5; i++)
            {
                xprev = x;
                var kprev = k;

                k = Math.Pow(k / (1 + Math.Sqrt(1 - k * k)), 2);

                x = 2 * x / ((1 + k) * (1 + Complex.Sqrt(1 - kprev * kprev * x * x)));
            }

            return 2 * Complex.Asin(x) / Math.PI;
        }
    }
}
