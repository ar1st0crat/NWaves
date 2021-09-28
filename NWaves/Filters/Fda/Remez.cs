using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Optimal equiripple filter designer based on Remez (Parks-McClellan) algorithm.
    /// <code>
    /// Example: <br/>
    /// <br/>
    ///     var order = 57; <br/>
    ///     var freqs = new double[] { 0, 0.15, 0.17, 0.5 }; <br/>
    ///     var response = new double[] { 1, 0 };            <br/>
    ///     var weights = new double[] { 0.01, 0.1 };        <br/>
    /// <br/>    
    ///     var remez = new Remez(order, freqs, response, weights); <br/>
    /// <br/>    
    ///     var kernel = remez.Design(); <br/>
    /// <br/>    
    ///     // We can monitor the following properties: <br/>
    /// <br/>
    ///     remez.Iterations           <br/>
    ///     remez.ExtremalFrequencies  <br/>
    ///     remez.InterpolatedResponse <br/>
    ///     remez.Error                <br/>
    /// </code>
    /// </summary>
    public class Remez
    {
        /// <summary>
        /// Gets filter order.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets number of actual iterations.
        /// </summary>
        public int Iterations { get; private set; }

        /// <summary>
        /// Gets number of extremal frequencies (K = Order/2 + 2).
        /// </summary>
        public int K { get; private set; }

        /// <summary>
        /// Gets interpolated frequency response.
        /// </summary>
        public double[] InterpolatedResponse { get; private set; }

        /// <summary>
        /// Gets array of errors.
        /// </summary>
        public double[] Error { get; private set; }

        /// <summary>
        /// Gets array of extremal frequencies.
        /// </summary>
        public double[] ExtremalFrequencies => _extrs.Select(e => _grid[e]).ToArray();
        
        /// <summary>
        /// Tolerance (for computing denominators).
        /// </summary>
        private const double Tolerance = 1e-7;

        /// <summary>
        /// Indices of extremal frequencies in the grid.
        /// </summary>
        private readonly int[] _extrs;

        /// <summary>
        /// Grid.
        /// </summary>
        private double[] _grid;

        /// <summary>
        /// Band edge frequencies.
        /// </summary>
        private readonly double[] _freqs;

        /// <summary>
        /// Desired frequency response on entire grid.
        /// </summary>
        private double[] _desired;

        /// <summary>
        /// Weights on entire grid.
        /// </summary>
        private double[] _weights;

        /// <summary>
        /// Points for interpolation.
        /// </summary>
        private readonly double[] _points;

        /// <summary>
        /// Gamma coefficients used in Lagrange interpolation.
        /// </summary>
        private readonly double[] _gammas;

        /// <summary>
        /// Precomputed cosines.
        /// </summary>
        private readonly double[] _cosTable;

        /// <summary>
        /// Constructs <see cref="Remez"/> filter designer.
        /// </summary>
        /// <param name="order">Order of filter</param>
        /// <param name="freqs">Array of normalized frequencies</param>
        /// <param name="desired">Array of desired response values at given frequencies</param>
        /// <param name="weights">Array of weights at given frequencies</param>
        /// <param name="gridDensity">Grid density</param>
        public Remez(int order, double[] freqs, double[] desired, double[] weights, int gridDensity = 16)
        {
            Guard.AgainstEvenNumber(order, "The order of the filter");
            Guard.AgainstIncorrectFilterParams(freqs, desired, weights);

            Order = order;

            K = Order / 2 + 2;

            _freqs = freqs;

            MakeGrid(desired, weights, gridDensity);

            InterpolatedResponse = new double[_grid.Length];
            Error = new double[_grid.Length];

            _extrs = new int[K];
            _points = new double[K];
            _gammas = new double[K];
            _cosTable = new double[K];
        }

        /// <summary>
        /// Creates grid (uniform in each band).
        /// </summary>
        /// <param name="desired">Array of desired response values</param>
        /// <param name="weights">Array of weights</param>
        /// <param name="gridDensity">Grid density</param>
        private void MakeGrid(double[] desired, double[] weights, int gridDensity = 16)
        {
            var gridSize = 0;
            var bandSizes = new int[_freqs.Length / 2];

            var step = 0.5 / (gridDensity * (K - 1));

            for (var i = 0; i < bandSizes.Length; i++)
            {
                bandSizes[i] = (int)((_freqs[2 * i + 1] - _freqs[2 * i]) / step + 0.5);

                gridSize += bandSizes[i];
            }

            _grid = new double[gridSize];
            _weights = new double[gridSize];
            _desired = new double[gridSize];

            var gi = 0;

            for (var i = 0; i < bandSizes.Length; i++)
            {
                var freq = _freqs[2 * i];

                for (var k = 0; k < bandSizes[i]; k++, gi++, freq += step)
                {
                    _grid[gi] = freq;
                    _weights[gi] = weights[i];
                    _desired[gi] = desired[i];
                }

                _grid[gi - 1] = _freqs[2 * i + 1];
            }
        }

        /// <summary>
        /// Uniform initialization of extremal frequencies.
        /// </summary>
        private void InitExtrema()
        {
            var n = _grid.Length;

            for (var k = 0; k < K; k++)
            {
                _extrs[k] = (int)(k * (n - 1.0) / (K - 1));
            }
        }

        /// <summary>
        /// Designs optimal equiripple filter and returns the kernel of designed filter.
        /// </summary>
        /// <param name="maxIterations">Max number of iterations</param>
        public double[] Design(int maxIterations = 100)
        {
            InitExtrema();

            var extrCandidates = new int[2 * K];

            for (Iterations = 0; Iterations < maxIterations; Iterations++)
            {
                // 1) Update gamma coefficients for extremal frequencies and interpolation points
                // 2) Compute delta

                UpdateCoefficients();

                // 3) interpolate: ==============================================================
                
                for (var i = 0; i < _grid.Length; i++)
                {
                    InterpolatedResponse[i] = Lagrange(_grid[i]);
                }

                // 4) evaluate error on entire grid: ============================================

                for (var i = 0; i < _grid.Length; i++)
                {
                    Error[i] = _weights[i] * (_desired[i] - InterpolatedResponse[i]);
                }

                // 5) find extrema in error function (array): ===================================

                var extrCount = 0;
                var n = _grid.Length;

                // first, simply find all peaks of error function
                // (alternation theorem guarantees that there'll be at least K peaks):

                if (Math.Abs(Error[0]) > Math.Abs(Error[1]))
                {
                    extrCandidates[extrCount++] = 0;
                }
                for (var i = 1; i < n - 1; i++)
                {
                    if ((Error[i] > 0.0 && Error[i] >= Error[i - 1] && Error[i] > Error[i + 1]) ||
                        (Error[i] < 0.0 && Error[i] <= Error[i - 1] && Error[i] < Error[i + 1]))
                    {
                        extrCandidates[extrCount++] = i;
                    }
                }
                if (Math.Abs(Error[n - 1]) > Math.Abs(Error[n - 2]))
                {
                    extrCandidates[extrCount++] = n - 1;
                }

                // less than K peaks? then algorithm's converged (theoretically)

                if (extrCount < K) break;


                // if there are more than K extrema, then remove the least important one by one
                // until we have the most important K extrema in the set:

                while (extrCount > K)
                {
                    // find index of peak with minimum abs error:

                    var indexToRemove = 0;

                    for (var i = 1; i < extrCount; i++)
                    {
                        if (Math.Abs(Error[extrCandidates[i]]) < Math.Abs(Error[extrCandidates[indexToRemove]]))
                        {
                            indexToRemove = i;
                        }
                    }

                    // remove extrCandidate with indexToRemove:

                    extrCount--;

                    for (var i = indexToRemove; i < extrCount; i++)
                    {
                        extrCandidates[i] = extrCandidates[i + 1];
                    }
                }

                Array.Copy(extrCandidates, _extrs, K);


                // 6) check if we should continue iterations: ==================================

                var maxError = Math.Abs(Error[0]);
                var minError = maxError;

                for (var k = 0; k < K; k++)
                {
                    var error = Math.Abs(Error[_extrs[k]]);

                    if (error < minError) minError = error;
                    if (error > maxError) maxError = error;
                }

                if ((maxError - minError) / minError < 1e-6) break;
            }


            // finally, compute impulse response from interpolated frequency response:

            return ImpulseResponse();
        }

        /// <summary>
        /// Updates gamma coefficients, interpolation points and delta.
        /// </summary>
        private void UpdateCoefficients()
        {
            // 0) update cos table for future calculations: ==============================

            for (int i = 0; i < _cosTable.Length; i++)
            {
                _cosTable[i] = Math.Cos(2 * Math.PI * _grid[_extrs[i]]);
            }

            // 1) compute gamma coefficients: ============================================

            var num = 0.0;
            var den = 0.0;

            for (int i = 0, sign = 1; i < K; i++, sign = -sign)
            {
                _gammas[i] = Gamma(i);

                num += _gammas[i] * _desired[_extrs[i]];
                den += sign * _gammas[i] / _weights[_extrs[i]];
            }

            // 2) compute delta: =========================================================

            var delta = num / den;

            // 3) compute points for interpolation: ======================================

            for (int i = 0, sign = 1; i < K; i++, sign = -sign)
            {
                _points[i] = _desired[_extrs[i]] - sign * delta / _weights[_extrs[i]];
            }
        }

        /// <summary>
        /// Reconstructs impulse response from interpolated frequency response.
        /// </summary>
        private double[] ImpulseResponse()
        {
            UpdateCoefficients();

            var halfOrder = Order / 2;

            // optional: pre-calculate lagrange interpolated values =====================

            var lagr = Enumerable.Range(0, halfOrder + 1)
                                 .Select(i => Lagrange((double)i / Order))
                                 .ToArray();

            // compute kernel (impulse response): =======================================

            var kernel = new double[Order];

            for (var k = 0; k < Order; k++)
            {
                var sum = 0.0;
                for (var i = 1; i <= halfOrder; i++)
                {
                    sum += lagr[i] * Math.Cos(2 * Math.PI * i * (k - halfOrder) / Order);
                }

                kernel[k] = (lagr[0] + 2 * sum) / Order;
            }

            return kernel;
        }

        /// <summary>
        /// Computes gamma coefficient.
        /// </summary>
        /// <param name="k">Input value</param>
        private double Gamma(int k)
        {
            var jet = (K - 1) / 15 + 1;     // as in original Rabiner's code; without it there'll be numerical issues 
            var den = 1.0;

            for (var j = 0; j < jet; j++)
            {
                for (var i = j; i < K; i += jet)
                {
                    if (i != k) den *= 2 * (_cosTable[k] - _cosTable[i]);
                }
            }

            if (Math.Abs(den) < Tolerance) den = Tolerance;

            return 1 / den;
        }

        /// <summary>
        /// Barycentric Lagrange interpolation.
        /// </summary>
        /// <param name="freq">Frequency</param>
        private double Lagrange(double freq)
        {
            var num = 0.0;
            var den = 0.0;

            var cosFreq = Math.Cos(2 * Math.PI * freq);

            for (var i = 0; i < K; i++)
            {
                var cosDiff = cosFreq - _cosTable[i];

                if (Math.Abs(cosDiff) < Tolerance) return _points[i];

                cosDiff = _gammas[i] / cosDiff;
                den += cosDiff;
                num += cosDiff * _points[i];
            }

            return num / den;
        }

        /// <summary>
        /// Convert ripple (in dB) to passband weight.
        /// </summary>
        /// <param name="db">Ripple (in dB)</param>
        public static double DbToPassbandWeight(double db) => (Math.Pow(10, db / 20) - 1) / (Math.Pow(10, db / 20) + 1);

        /// <summary>
        /// Converts ripple (in dB) to stopband weight.
        /// </summary>
        /// <param name="ripple">Ripple (in dB)</param>
        public static double DbToStopbandWeight(double ripple) => Math.Pow(10, -ripple / 20);

        /// <summary>
        /// Estimates order of a low-pass filter.
        /// </summary>
        /// <param name="fp">Passband edge frequency</param>
        /// <param name="fa">Stopband edge frequency</param>
        /// <param name="dp">Passband weight</param>
        /// <param name="da">Stopband weight</param>
        public static int EstimateOrder(double fp, double fa, double dp, double da)
        {
            // Estimates LP filter order according to [Herrman et al., 1973].
            // Section 8.2.7 in Proakis and Manolakis book.
            
            if (dp < da)
            {
                var tmp = dp;
                dp = da;
                da = tmp;
            }

            var bw = fa - fp;

            var d = (0.005309 * Math.Log10(dp) * Math.Log10(dp) + 0.07114 * Math.Log10(dp) - 0.4761) * Math.Log10(da) -
                    (0.00266 * Math.Log10(dp) * Math.Log10(dp) + 0.5941 * Math.Log10(dp) + 0.4278);

            var f = 0.51244 * (Math.Log10(dp) - Math.Log10(da)) + 11.012;

            var l = (int)((d - f * bw * bw) / bw + 1.5);

            return l % 2 == 1 ? l : l + 1;
        }

        /// <summary>
        /// Estimates order of a filter with custom bands. 
        /// 
        /// Parameters are given in conventional format. For example:
        /// 
        /// <code>
        ///     freqs: { 0, 0.2, 0.22, 0.32, 0.33, 0.5 }
        ///     deltas: { 0.01, 0.1, 0.06 }
        /// </code>
        /// </summary>
        /// <param name="freqs">Array of edge frequencies</param>
        /// <param name="deltas">Array of weights</param>
        public static int EstimateOrder(double[] freqs, double[] deltas)
        {
            var maxOrder = 0;

            for (int fi = 1, di = 0; di < deltas.Length - 1; fi += 2, di++)
            {
                var order = EstimateOrder(freqs[fi], freqs[fi + 1], deltas[di], deltas[di + 1]);

                if (order > maxOrder)
                {
                    maxOrder = order;
                }
            }

            return maxOrder;
        }
    }
}
