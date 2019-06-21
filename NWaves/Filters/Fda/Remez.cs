using NWaves.Filters.Base;
using System;
using System.Linq;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Optimal equiripple filter designer based on Remez (Parks-McClellan) algorithm
    /// </summary>
    public class Remez
    {
        public int Order { get; private set; }
        public int K { get; private set; }

        private readonly double _fp;
        private readonly double _fa;
        private readonly double _d1;
        private readonly double _d2;

        /// <summary>
        /// Grid frequencies (including transition bands)
        /// </summary>
        private readonly double[] _grid;

        /// <summary>
        /// Extremal frequencies
        /// </summary>
        public readonly double[] _extrs;

        /// <summary>
        /// Interpolated frequency response
        /// </summary>
        public readonly double[] _freqResponse;

        /// <summary>
        /// Points for interpolation
        /// </summary>
        private readonly double[] _points;

        /// <summary>
        /// 
        /// </summary>
        private readonly double[] _betas;

        /// <summary>
        /// Array of errors
        /// </summary>
        public readonly double[] _error;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fp"></param>
        /// <param name="fa"></param>
        /// <param name="ripplePass"></param>
        /// <param name="rippleStop"></param>
        /// <param name="order"></param>
        public Remez(double fp, double fa, double ripplePass, double rippleStop, int order = 0)
        {
            Order = order > 0 ? order : EstimateOrder();

            _fp = fp;// 0.175;
            _fa = fa;// 0.185;
            _d1 = ripplePass; // (10 ** (50/20) - 1) / (10 ** (50/20) + 1)
            _d2 = rippleStop; // 10 ** (-20/20)

            var n = 8 * Order;

            var L = (Order - 1) / 2;
            K = L + 3;

            // ========================== Make initial grid: =========================

            var r = (Order + 1) / 2;
            var b1 = fp;
            var b2 = 0.5 - fa;
            var w0 = (b1 + b2) / (r - 1);
            var m1 = b1 / w0 + 0.5;
            var m2 = r - m1;
            var w1 = b1 / m1;
            var w2 = b2 / m2;

            var step = 0.5 / (n - 1);
            _grid = Enumerable.Range(0, n).Select(g => g * step).ToArray();

            _extrs = new double[K];

            var i = 0;
            var freq = 0.0;
            while (freq <= fp)
            {
                _extrs[i++] = freq;
                freq += w1;
            }
            freq = fa;
            while (freq < 0.5)
            {
                _extrs[i++] = freq;
                freq += w2;
            }
            _extrs[_extrs.Length - 1] = 0.5;

            // =======================================================================

            _points = new double[K];
            _betas = new double[K];
            _freqResponse = new double[n];
            _error = new double[n];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public FirFilter Design()
        {
            var n = _grid.Length;

            var prevDelta = 1.0;

            for (var iter = 0; iter < 50; iter++)
            {
                // 1) compute delta: ===========================================

                var num = 0.0;
                var den = 0.0;
                var sign = 1;

                for (var i = 0; i < K; i++)
                {
                    var gamma = Gamma(i, K);

                    var desired = _extrs[i] <= _fp ? 1 : 0;
                    var weightInv = _extrs[i] <= _fp ? _d1 / _d2 : 1;
                    num += gamma * desired;
                    den += sign * gamma * weightInv;
                    sign = -sign;
                }

                var delta = num / den;

                // 2) compute points for interpolation: ============================

                sign = 1;
                for (var i = 0; i < K; i++)
                {
                    var desired = _extrs[i] <= _fp ? 1 : 0;
                    var weightInv = _extrs[i] <= _fp ? _d1 / _d2 : 1;

                    _points[i] = desired - sign * delta * weightInv;
                    sign = -sign;

                    _betas[i] = Gamma(i, K - 1);
                }

                // 3) interpolate: =================================================

                for (var i = 0; i < _freqResponse.Length; i++)
                {
                    _freqResponse[i] = -1;
                }
                var epos = 0;
                for (var i = 0; i < K; i++)
                {
                    var pos = Math.Min((int)(2 * _extrs[i] * n), n - 1);
                    _freqResponse[pos] = _points[epos++];

                    //if (epos == K) break;
                }

                for (var i = 0; i < _grid.Length; i++)
                {
                    if (_freqResponse[i] == -1) _freqResponse[i] = Lagrange(_grid[i], K - 1);
                }

                // 4) evaluate error (excluding transition band): ==================

                for (var i = 0; i < _grid.Length; i++)
                {
                    if (_grid[i] < _fp || _grid[i] > _fa)
                    {
                        var desired = _grid[i] <= _fp ? 1 : 0;
                        var weight = _grid[i] <= _fp ? _d2 / _d1 : 1;

                        _error[i] = Math.Abs(weight * (desired - _freqResponse[i]));
                    }
                }

                // 5) find extrema in error function (array): ==================================

                // don't touch first and last extrema positions, so start k from 1

                var k = 1;
                for (var i = 1; i < _error.Length - 1; i++)
                {
                    if (_error[i] > _error[i - 1] && _error[i] > _error[i + 1] && k < _extrs.Length)
                    {
                        _extrs[k++] = _grid[i++];  // insert to extrs
                    }
                }
                if (k < _extrs.Length) _extrs[k++] = _grid[n - 1];

                K = Math.Min(k, K);


                // 6) check if we should continue iterations: ==================================

                // actually the stopping condition is different, but this one is also OK:

                if (Math.Abs(delta - prevDelta) < 1e-24) break;

                prevDelta = delta;
            }


            // finally, compute impulse response from interpolated frequency response:

            var kernel = new double[Order];

            var halfOrder = Order / 2 + 1;

            var lagr = new double[halfOrder];
            for (var i = 1; i < halfOrder; i++)
            {
                lagr[i] = Lagrange((double)i / Order, K - 1);
            }

            for (var k = 0; k < halfOrder; k++)
            {
                var sum = 0.0;
                for (var i = 1; i < halfOrder; i++)
                {
                    sum += lagr[i] * Math.Cos(2 * Math.PI * i * k / Order);
                }

                kernel[halfOrder - 1 + k] = kernel[halfOrder - 1 - k] = (_freqResponse[0] + 2 * sum) / Order;
            }

            return new FirFilter(kernel);
        }

        public int EstimateOrder()
        {
            return 0;
        }

        private double CosDifference(double f1, double f2) => Math.Cos(2 * Math.PI * f1) - Math.Cos(2 * Math.PI * f2);

        private double Gamma(int k, int n)
        {
            var prod = 1.0;
            for (var i = 0; i < n; i++)
            {
                if (i != k) prod *= 1 / CosDifference(_extrs[k], _extrs[i]);
            }
            return prod;
        }

        private double Lagrange(double freq, int n)
        {
            var num = 0.0;
            var den = 0.0;

            for (var k = 0; k < n; k++)
            {
                num += _points[k] * _betas[k] / CosDifference(freq, _extrs[k]);
                den += _betas[k] / CosDifference(freq, _extrs[k]);
            }

            return num / den;
        }
    }
}
