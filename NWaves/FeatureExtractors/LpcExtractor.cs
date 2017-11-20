using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
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
        /// <param name="samplingRate"></param>
        /// <param name="windowSize"></param>
        /// <param name="overlapSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpcExtractor(int order, int samplingRate, double windowSize = 0.0256, double overlapSize = 0.010,
                            double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            _order = order;

            _windowSize = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, _windowSize);
            _window = window;

            _hopSize = (int)(samplingRate * overlapSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            // for advanced Levinson-Durbin
            _tmpLevinsonBuffer = new double[_order + 1];
        }

        /// <summary>
        /// Standard method for computing LPC features.
        /// This version is 'easy-to-read' and intended for understanding method's idea.
        /// 
        /// Note:
        ///     The first LP coefficient is always equal to 1.0.
        ///     This method replaces it with the value of prediction error.
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns>List of LPC vectors</returns>
        public IEnumerable<FeatureVector> ComputeFromEasyToRead(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;

            var block = new DiscreteSignal(signal.SamplingRate, new double[_windowSize]);

            var i = 0;
            while (i + _windowSize < filtered.Length)
            {
                FastCopy.ToExistingArray(filtered.Samples, block.Samples, _windowSize, i);

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }

                // 2) autocorrelation

                var cc = Operation.CrossCorrelate(block, block).Last(_windowSize);

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
        /// Slightly optimized version (with no unnecessary memory allocations).
        /// This version is used in production.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            var fftSize = MathUtils.NextPowerOfTwo(2 * _windowSize - 1);

            var block = new double[fftSize];
            var reversed = new double[fftSize];
            var zeroblock = new double[fftSize];
            var blockImag = new double[fftSize];
            var reversedImag = new double[fftSize];

            var ccr = new double[fftSize];
            var cci = new double[fftSize];
            var cc = new double[_windowSize];

            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;

            var i = 0;
            while (i + _windowSize < filtered.Length)
            {
                FastCopy.ToExistingArray(zeroblock, block, fftSize);
                FastCopy.ToExistingArray(zeroblock, reversed, fftSize);
                FastCopy.ToExistingArray(zeroblock, blockImag, fftSize);
                FastCopy.ToExistingArray(zeroblock, reversedImag, fftSize);
                FastCopy.ToExistingArray(filtered.Samples, block, _windowSize, i);

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }

                // 2) autocorrelation

                for (var j = 0; j < _windowSize; j++)
                {
                    reversed[j] = block[_windowSize - 1 - j];
                }

                Transform.Fft(block, blockImag, fftSize);
                Transform.Fft(reversed, reversedImag, fftSize);

                for (var j = 0; j < fftSize; j++)
                {
                    ccr[j] = (block[j] * reversed[j] - blockImag[j] * reversedImag[j]) / fftSize;
                    cci[j] = (block[j] * reversedImag[j] + reversed[j] * blockImag[j]) / fftSize;
                }

                Transform.Ifft(ccr, cci, fftSize);
                FastCopy.ToExistingArray(ccr, cc, _windowSize, _windowSize - 1);

                // 3) levinson-durbin

                var a = new double[_order + 1];
                var err = LevinsonDurbin(cc, a);
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
