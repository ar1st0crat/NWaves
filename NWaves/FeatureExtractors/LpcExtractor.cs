using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms.Windows;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Linear Predictive Coding coefficients extractor
    /// </summary>
    public class LpcExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of features coincides with the order of LPC
        /// </summary>
        public override int FeatureCount => _order;

        /// <summary>
        /// Descriptions ("error", "lpc1", "lpc2", etc.)
        /// </summary>
        public override IEnumerable<string> FeatureDescriptions
        {
            get
            {
                return new [] { "error" }.Concat(
                    Enumerable.Range(1, FeatureCount).Select(i => "lpc" + i));
            }
        }
            
        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        private readonly int _order;

        /// <summary>
        /// Size of analysis window
        /// </summary>
        private readonly int _windowSize;

        /// <summary>
        /// Size of overlap
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Samples of the window
        /// </summary>
        private readonly double[] _windowSamples;

        /// <summary>
        /// Pre-emphasis filter (if needed)
        /// </summary>
        private readonly PreEmphasisFilter _preemphasisFilter;

        /// <summary>
        /// Array used in andvanced Levinson-Durbin recursive algorithm
        /// </summary>
        private readonly double[] _tmpLevinsonBuffer;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpcExtractor(int order, int windowSize = 512, int hopSize = 256,
                            double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            _order = order;
            _windowSize = windowSize;
            _hopSize = hopSize;
            _window = window;
            _windowSamples = Window.OfType(window, windowSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            // for advanced Levinson-Durbin
            _tmpLevinsonBuffer = new double[_order + 1];
        }

        /// <summary>
        /// Standard method for computing LPC features.
        /// 
        /// Note:
        ///     The first LP coefficient is always equal to 1.0.
        ///     This method replaces it with the value of prediction error.
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns>List of LPC vectors</returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;

            var i = 0;
            while (i + _windowSize < filtered.Samples.Length)
            {
                var x = filtered[i, i + _windowSize];

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    x.ApplyWindow(_windowSamples);
                }

                // 2) autocorrelation

                var cc = Operation.CrossCorrelate(x, x).Last(_windowSize);

                // 3) levinson-durbin

                var a = new double[_order + 1];
                var err = LevinsonDurbin(cc.Samples, a);
                a[0] = err;

                // add LPC vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = a,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _hopSize;
            }

            return featureVectors;
        }
        
        /// <summary>
        /// Levinson-Durbin algorithm for solving main LPC task
        /// </summary>
        /// <param name="input">Auto-correlation vector</param>
        /// <param name="a">LP coefficients</param>
        /// <returns>Prediction error</returns>
        public double LevinsonDurbin(double[] input, double[] a)
        {
            var err = input[0];

            a[0] = 1.0;

            for (var i = 1; i <= _order; i++)
            {
                var lambda = 0.0;
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

                err *= (1.0 - lambda * lambda);
            }

            return err;
        }

        /// <summary>
        /// Advanced version of Levinson-Durbin recursion:
        /// it additionally calculates reflection coefficients and uses some temporary buffer
        /// </summary>
        /// <param name="input">Auto-correlation vector</param>
        /// <param name="a">LP coefficients</param>
        /// <param name="k">Reflection coefficients</param>
        /// <returns></returns>
        public double LevinsonDurbinAdvanced(double[] input, double[] a, double[] k)
        {
            for (var i = 0; i <= _order; i++)
            {
                _tmpLevinsonBuffer[i] = 0.0;
            }

            var err = input[0];

            a[0] = 1.0;

            for (var i = 1; i <= _order; ++i)
            {
                var acc = input[i];

                for (var j = 1; j <= i - 1; ++j)
                {
                    acc += a[j] * input[i - j];
                }

                k[i - 1] = -acc / err;
                a[i] = k[i - 1];

                for (var j = 0; j < _order; ++j)
                {
                    _tmpLevinsonBuffer[j] = a[j];
                }

                for (var j = 1; j < i; ++j)
                {
                    a[j] += k[i - 1] * _tmpLevinsonBuffer[i - j];
                }

                err *= (1 - k[i - 1] * k[i - 1]);
            }

            return err;
        }

        /// <summary>
        /// Method returns LPC order for a given sampling rate 
        /// according to the best practices.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <returns>LPC order</returns>
        public int EstimateOrder(int samplingRate)
        {
            return 2 + samplingRate / 1000;
        }
    }
}
