using NWaves.Filters.Base;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Optimal equiripple LP filter designer based on Remez (Parks-McClellan) algorithm.
    /// This is simplified version of the universal Remez designer (Remez.cs).
    /// The code is cleaner, so it's good for educational purpose.
    /// </summary>
    public class RemezLp
    {
        /// <summary>
        /// Filter order
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Iterations { get; private set; }

        /// <summary>
        /// Number of extremal frequencies
        /// </summary>
        public int L { get; private set; }

        /// <summary>
        /// Extremal frequencies
        /// </summary>
        public double[] Extrs { get; private set; }

        /// <summary>
        /// Array of errors
        /// </summary>
        public double[] Error { get; private set; }

        /// <summary>
        /// Interpolated frequency response
        /// </summary>
        public double[] FrequencyResponse { get; private set; }

        /// <summary>
        /// Transition band left frequency
        /// </summary>
        private readonly double _fp;

        /// <summary>
        /// Transition band right frequency
        /// </summary>
        private readonly double _fa;

        /// <summary>
        /// Ripple in the passband
        /// </summary>
        private readonly double _d1;

        /// <summary>
        /// Ripple in the stopband
        /// </summary>
        private readonly double _d2;

        /// <summary>
        /// Grid frequencies (including transition bands)
        /// </summary>
        private readonly double[] _grid;

        /// <summary>
        /// Points for interpolation
        /// </summary>
        private readonly double[] _points;

        /// <summary>
        /// Beta coefficients used in Lagrange interpolation
        /// </summary>
        private readonly double[] _betas;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fp"></param>
        /// <param name="fa"></param>
        /// <param name="ripplePass"></param>
        /// <param name="rippleStop"></param>
        /// <param name="order"></param>
        /// <param name="gridDensity"></param>
        public RemezLp(double fp, double fa, double ripplePass, double rippleStop, int order = 0, int gridDensity = 8)
        {
            Order = order > 0 ? order : EstimateOrder(fp, fa, ripplePass, rippleStop);

            Guard.AgainstEvenNumber(order, "The order of the filter");

            _fp = fp;
            _fa = fa;
            _d1 = (Math.Pow(10, ripplePass / 20) - 1) / (Math.Pow(10, ripplePass / 20) + 1);
            _d2 = Math.Pow(10, -rippleStop / 20);

            var n = Order * gridDensity;

            var step = 0.5 / (n - 1);
            _grid = Enumerable.Range(0, n).Select(g => g * step).ToArray();

            L = (Order - 1) / 2 + 3;

            FrequencyResponse = new double[n];
            Error = new double[n];
            Extrs = new double[L];
            _points = new double[L];
            _betas = new double[L];
        }

        /// <summary>
        /// Uniform initialization of extremal frequencies
        /// </summary>
        public void InitExtrema()
        {
            var r = (Order + 1) / 2;
            var b1 = _fp;
            var b2 = 0.5 - _fa;
            var w0 = (b1 + b2) / (r - 1);
            var m1 = b1 / w0 + 0.5;
            var m2 = r - m1;
            var w1 = b1 / m1;
            var w2 = b2 / m2;

            var i = 0;
            var freq = 0.0;
            while (freq <= _fp)
            {
                Extrs[i++] = freq;
                freq += w1;
            }
            freq = _fa;
            while (freq < 0.5)
            {
                Extrs[i++] = freq;
                freq += w2;
            }
            Extrs[Extrs.Length - 1] = 0.5;

            // reset L:

            L = (Order - 1) / 2 + 3;
        }

        /// <summary>
        /// Design optimal equiripple filter
        /// </summary>
        /// <param name="maxIterations">Max number of iterations</param>
        /// <returns></returns>
        public FirFilter Design(int maxIterations = 150)
        {
            InitExtrema();

            var n = _grid.Length;

            var prevDelta = 1.0;

            for (Iterations = 0; Iterations < maxIterations; Iterations++)
            {
                // 1) compute delta: ===========================================

                var num = 0.0;
                var den = 0.0;
                var sign = 1;

                for (var i = 0; i < L; i++)
                {
                    var gamma = Gamma(i, L);

                    var desired = Extrs[i] <= _fp ? 1 : 0;
                    var weightInv = Extrs[i] <= _fp ? _d1 / _d2 : 1;
                    num += gamma * desired;
                    den += sign * gamma * weightInv;
                    sign = -sign;
                }

                var delta = num / den;

                // 2) compute points for interpolation: ============================

                sign = 1;
                for (var i = 0; i < L; i++)
                {
                    var desired = Extrs[i] <= _fp ? 1 : 0;
                    var weightInv = Extrs[i] <= _fp ? _d1 / _d2 : 1;

                    _points[i] = desired - sign * delta * weightInv;
                    sign = -sign;

                    _betas[i] = Gamma(i, L - 1);
                }

                // 3) interpolate: =================================================

                for (var i = 0; i < FrequencyResponse.Length; i++)
                {
                    FrequencyResponse[i] = -1;
                }
                var epos = 0;
                for (var i = 0; i < L; i++)
                {
                    var pos = Math.Min((int)(2 * Extrs[i] * n), n - 1);
                    FrequencyResponse[pos] = _points[epos++];
                }

                for (var i = 0; i < _grid.Length; i++)
                {
                    if (FrequencyResponse[i] == -1) FrequencyResponse[i] = Lagrange(_grid[i], L - 1);
                }

                // 4) evaluate error (excluding transition band): ==================

                for (var i = 0; i < _grid.Length; i++)
                {
                    if (_grid[i] < _fp || _grid[i] > _fa)
                    {
                        var desired = _grid[i] <= _fp ? 1 : 0;
                        var weight = _grid[i] <= _fp ? _d2 / _d1 : 1;

                        Error[i] = Math.Abs(weight * (desired - FrequencyResponse[i]));
                    }
                }

                // 5) find extrema in error function (array): ==================================

                // don't touch first and last extrema positions, so start k from 1

                var k = 1;
                for (var i = 1; i < Error.Length - 1; i++)
                {
                    if (Error[i] > Error[i - 1] && Error[i] > Error[i + 1] && k < Extrs.Length)
                    {
                        Extrs[k++] = _grid[i++];  // insert to extrs
                    }
                }
                if (k < Extrs.Length)
                {
                    Extrs[k++] = _grid[n - 1];
                }

                L = Math.Min(k, L);

                Extrs[L - 1] = _grid[n - 1];


                // 6) check if we should continue iterations: ==================================

                // actually the stopping condition is different, but this one is also OK:

                if (Math.Abs(delta - prevDelta) < 1e-25) break;

                prevDelta = delta;
            }


            // finally, compute impulse response from interpolated frequency response:

            var kernel = new double[Order];

            var halfOrder = Order / 2 + 1;

            // optional: pre-calculate lagrange interpolated values ============

            var lagr = new double[halfOrder];
            for (var i = 1; i < halfOrder; i++)
            {
                lagr[i] = Lagrange((double)i / Order, L - 1);
            }
            // =================================================================

            for (var k = 0; k < halfOrder; k++)
            {
                var sum = 0.0;
                for (var i = 1; i < halfOrder; i++)
                {
                    sum += lagr[i] * Math.Cos(2 * Math.PI * i * k / Order);
                }

                kernel[halfOrder - 1 + k] = kernel[halfOrder - 1 - k] = (FrequencyResponse[0] + 2 * sum) / Order;
            }

            return new FirFilter(kernel);
        }

        /// <summary>
        /// Estimate filter order according to [Herrman et al., 1973].
        /// LowPass only!
        /// Section 8.2.7 in Proakis & Manolakis book.
        /// </summary>
        /// <param name="fp"></param>
        /// <param name="fa"></param>
        /// <param name="ripplePass"></param>
        /// <param name="rippleStop"></param>
        /// <returns></returns>
        public static int EstimateOrder(double fp, double fa, double ripplePass, double rippleStop)
        {
            var rp = (Math.Pow(10, ripplePass / 20) - 1) / (Math.Pow(10, ripplePass / 20) + 1);
            var rs = Math.Pow(10, -rippleStop / 20);

            if (rp < rs)
            {
                var tmp = rp;
                rp = rs;
                rs = tmp;
            }

            var bw = fa - fp;

            var d = (0.005309 * Math.Log10(rp) * Math.Log10(rp) + 0.07114 * Math.Log10(rp) - 0.4761) * Math.Log10(rs) -
                    (0.00266  * Math.Log10(rp) * Math.Log10(rp) + 0.5941  * Math.Log10(rp) + 0.4278);

            var f = 0.51244 * (Math.Log10(rp) - Math.Log10(rs)) + 11.012;

            var l = (int)((d - f * bw * bw) / bw + 1.5);

            return l % 2 == 1 ? l : l + 1;
        }


        #region helping functions

        private double CosDifference(double f1, double f2) => Math.Cos(2 * Math.PI * f1) - Math.Cos(2 * Math.PI * f2);

        private double Gamma(int k, int n)
        {
            var prod = 1.0;
            for (var i = 0; i < n; i++)
            {
                if (i != k) prod *= 1 / CosDifference(Extrs[k], Extrs[i]);
            }
            return prod;
        }

        private double Lagrange(double freq, int n)
        {
            var num = 0.0;
            var den = 0.0;

            for (var k = 0; k < n; k++)
            {
                num += _points[k] * _betas[k] / CosDifference(freq, Extrs[k]);
                den += _betas[k] / CosDifference(freq, Extrs[k]);
            }

            return num / den;
        }

        #endregion
    }
}
