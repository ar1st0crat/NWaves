using NWaves.Filters.Base;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Optimal equiripple filter designer based on Remez (Parks-McClellan) algorithm.
    /// Supports all band forms: LP, HP, BP, BS.
    /// </summary>
    public class Remez
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
        /// Passband / stopband frequencies
        /// </summary>
        private readonly double[] _freqs;

        /// <summary>
        /// Passband / stopband frequencies (one freq in LP/HP case and two freqs in BP/BS case)
        /// </summary>
        private readonly double _fp1, _fp2, _fa1, _fa2;

        /// <summary>
        /// Ripples in bands (two ripples in LP/HP case and three ripples in BP/BS case)
        /// </summary>
        private readonly double _d1, _d2, _d3;

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
        /// <param name="freqs"></param>
        /// <param name="ripples"></param>
        /// <param name="order"></param>
        /// <param name="type"></param>
        /// <param name="gridDensity"></param>
        public Remez(double[] freqs, double[] ripples, int order, BandForm type = BandForm.LowPass, int gridDensity = 8)
        {
            Guard.AgainstEvenNumber(order, "The order of the filter");
            Order = order;

            _freqs = freqs;

            // make uniform frequency grid: ===============================

            var n = Order * gridDensity;

            var step = 0.5 / (n - 1);
            _grid = Enumerable.Range(0, n).Select(g => g * step).ToArray();

            // ============================================================

            switch (type)
            {
                case BandForm.LowPass:
                    _fp1 = freqs[1];
                    _fa1 = freqs[2];
                    _d1 = PassbandDeltaFromDb(ripples[0]);
                    _d2 = StopbandDeltaFromDb(ripples[1]);
                    EvaluateError = EvaluateErrorLp;
                    ExtremalResponse = ExtremalResponseLp;
                    ErrorWeight = ErrorWeightLp;
                    break;
                case BandForm.HighPass:
                    _fa1 = freqs[1];
                    _fp1 = freqs[2];
                    _d1 = StopbandDeltaFromDb(ripples[0]);
                    _d2 = PassbandDeltaFromDb(ripples[1]);
                    EvaluateError = EvaluateErrorHp;
                    ExtremalResponse = ExtremalResponseHp;
                    ErrorWeight = ErrorWeightHp;
                    break;
                case BandForm.BandPass:
                    _fa1 = freqs[1];
                    _fp1 = freqs[2];
                    _fp2 = freqs[3];
                    _fa2 = freqs[4];
                    _d1 = StopbandDeltaFromDb(ripples[0]);
                    _d2 = PassbandDeltaFromDb(ripples[1]);
                    _d3 = StopbandDeltaFromDb(ripples[2]);
                    EvaluateError = EvaluateErrorBp;
                    ExtremalResponse = ExtremalResponseBp;
                    ErrorWeight = ErrorWeightBp;
                    break;
                case BandForm.BandStop:
                    _fp1 = freqs[1];
                    _fa1 = freqs[2];
                    _fa2 = freqs[3];
                    _fp2 = freqs[4];
                    _d1 = PassbandDeltaFromDb(ripples[0]);
                    _d2 = StopbandDeltaFromDb(ripples[1]);
                    _d3 = PassbandDeltaFromDb(ripples[2]);
                    EvaluateError = EvaluateErrorBs;
                    ExtremalResponse = ExtremalResponseBs;
                    ErrorWeight = ErrorWeightBs;
                    break;
            }

            L = (Order - 1) / 2 + _freqs.Length / 2 + 1;

            FrequencyResponse = new double[n];
            Error = new double[n];
            Extrs = new double[L];
            _points = new double[L];
            _betas = new double[L];
        }

        /// <summary>
        /// Uniform initialization of extremal frequencies
        /// </summary>
        private void InitExtrema()
        {
            L = (Order - 1) / 2 + _freqs.Length / 2 + 1;
            
            var bw = new double[_freqs.Length / 2];
            var m = new double[bw.Length];
            var w = new double[bw.Length];

            for (var k = 0; k < bw.Length; k++)
            {
                bw[k] = _freqs[2 * k + 1] - _freqs[2 * k];
            }

            // uniform extrema in each band:

            var i = 0;
            var sum = 0.0;
            for (; i < bw.Length - 1; i++)
            {
                m[i] = (int)(2 * bw[i] * L);
                sum += m[i];
            }
            m[i] = L - sum;

            var j = 0;
            for (i = 0; i < bw.Length; i++)
            {
                w[i] = bw[i] / (m[i] - 1);

                for (var k = 0; k < m[i]; k++)
                {
                    Extrs[j++] = _freqs[2 * i] + w[i] * k;
                }
            }
        }

        /// <summary>
        /// Design optimal equiripple filter
        /// </summary>
        /// <param name="maxIterations">Max number of iterations</param>
        /// <returns></returns>
        public TransferFunction Design(int maxIterations = 150)
        {
            InitExtrema();

            var n = _grid.Length;

            var prevDelta = 1.0;
            
            for (Iterations = 0; Iterations < maxIterations; Iterations++)
            {
                // 1) compute delta: =======================================================

                var num = 0.0;
                var den = 0.0;
                var sign = 1;

                for (var i = 0; i < L; i++)
                {
                    var gamma = Gamma(i, L);

                    var desired = ExtremalResponse(i);
                    var weightInv = ErrorWeight(i);
                    num += gamma * desired;
                    den += sign * gamma * weightInv;
                    sign = -sign;
                }

                var delta = num / den;

                // 2) compute points for interpolation: ======================================

                sign = 1;
                for (var i = 0; i < L; i++)
                {
                    var desired = ExtremalResponse(i);
                    var weightInv = ErrorWeight(i);

                    _points[i] = desired - sign * delta * weightInv;
                    sign = -sign;

                    _betas[i] = Gamma(i, L - 1);
                }

                // 3) interpolate: ============================================================

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

                // 4) evaluate error (excluding transition bands): =============================

                EvaluateError();

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

            return new TransferFunction(kernel, new[] { 1.0 });
        }


        /// <summary>
        /// Estimate filter order according to [Herrman et al., 1973].
        /// Section 8.2.7 in Proakis & Manolakis book.
        /// </summary>
        /// <param name="fp"></param>
        /// <param name="fa"></param>
        /// <param name="ripplePass"></param>
        /// <param name="rippleStop"></param>
        /// <returns></returns>
        public static int EstimateOrder(double fp, double fa, double ripplePass, double rippleStop)
        {
            var rp = PassbandDeltaFromDb(ripplePass);
            var rs = StopbandDeltaFromDb(rippleStop);

            if (rp < rs)
            {
                var tmp = rp;
                rp = rs;
                rs = tmp;
            }

            var bw = fa - fp;

            var d = (0.005309 * Math.Log10(rp) * Math.Log10(rp) + 0.07114 * Math.Log10(rp) - 0.4761) * Math.Log10(rs) -
                    (0.00266 * Math.Log10(rp) * Math.Log10(rp) + 0.5941 * Math.Log10(rp) + 0.4278);

            var f = 0.51244 * (Math.Log10(rp) - Math.Log10(rs)) + 11.012;

            var l = (int)((d - f * bw * bw) / bw + 1.5);

            return l % 2 == 1 ? l : l + 1;
        }

        public static double PassbandDeltaFromDb(double db) => (Math.Pow(10, db / 20) - 1) / (Math.Pow(10, db / 20) + 1);

        public static double StopbandDeltaFromDb(double db) => Math.Pow(10, -db / 20);


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


        #region Code that depends on the band form

        private readonly Func<int, int> ExtremalResponse;

        private int ExtremalResponseLp(int idx) => Extrs[idx] <= _fp1 ? 1 : 0;
        private int ExtremalResponseHp(int idx) => Extrs[idx] >= _fp1 ? 1 : 0;
        private int ExtremalResponseBp(int idx) => Extrs[idx] >= _fp1 && Extrs[idx] <= _fp2 ? 1 : 0;
        private int ExtremalResponseBs(int idx) => Extrs[idx] <= _fp1 || Extrs[idx] >= _fp2 ? 1 : 0;

        private readonly Func<int, double> ErrorWeight;

        private double ErrorWeightLp(int idx) => Extrs[idx] <= _fp1 ? _d1 : _d2;
        private double ErrorWeightHp(int idx) => Extrs[idx] >= _fp1 ? _d1 : _d2;
        private double ErrorWeightBp(int idx) => Extrs[idx] <= _fp1 ? _d1 : Extrs[idx] <= _fp2 ? _d2 : _d3;
        private double ErrorWeightBs(int idx) => Extrs[idx] <= _fp1 ? _d1 : Extrs[idx] <= _fp2 ? _d2 : _d3;

        private readonly Action EvaluateError;

        /// <summary>
        /// Evaluate error for LP band form
        /// </summary>
        private void EvaluateErrorLp()
        {
            for (var i = 0; i < _grid.Length; i++)
            {
                if (_grid[i] < _fp1 || _grid[i] > _fa1)
                {
                    var desired = _grid[i] <= _fp1 ? 1 : 0;
                    var weight = _grid[i] <= _fp1 ? _d2 / _d1 : 1;

                    Error[i] = Math.Abs(weight * (desired - FrequencyResponse[i]));
                }
            }
        }

        /// <summary>
        /// Evaluate error for HP band form
        /// </summary>
        private void EvaluateErrorHp()
        {
            for (var i = 0; i < _grid.Length; i++)
            {
                if (_grid[i] > _fp1 || _grid[i] < _fa1)
                {
                    var desired = _grid[i] >= _fp1 ? 1 : 0;
                    var weight = _grid[i] >= _fp1 ? _d1 / _d2 : 1;

                    Error[i] = Math.Abs(weight * (desired - FrequencyResponse[i]));
                }
            }
        }

        /// <summary>
        /// Evaluate error for BP band form
        /// </summary>
        private void EvaluateErrorBp()
        {
            int desired;
            double weight;

            for (var i = 0; i < _grid.Length; i++)
            {
                if (_grid[i] < _fa1)
                {
                    desired = 0;
                    weight = _d1;
                }
                else if (_grid[i] > _fp1 && _grid[i] < _fp2)
                {
                    desired = 1;
                    weight = _d2;
                }
                else if (_grid[i] > _fa2)
                {
                    desired = 0;
                    weight = _d3;
                }
                else continue;

                Error[i] = Math.Abs(weight * (desired - FrequencyResponse[i]));
            }
        }

        /// <summary>
        /// Evaluate error for BS band form
        /// </summary>
        private void EvaluateErrorBs()
        {
            int desired;
            double weight;

            for (var i = 0; i < _grid.Length; i++)
            {
                if (_grid[i] < _fp1)
                {
                    desired = 1;
                    weight = _d1;
                }
                else if (_grid[i] > _fa1 && _grid[i] < _fa2)
                {
                    desired = 0;
                    weight = _d2;
                }
                else if (_grid[i] > _fp2)
                {
                    desired = 1;
                    weight = _d3;
                }
                else continue;

                Error[i] = Math.Abs(weight * (desired - FrequencyResponse[i]));
            }
        }

        #endregion
    }
}
