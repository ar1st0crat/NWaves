using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Operations;
using NWaves.Signals;
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
        public override string[] FeatureDescriptions
        {
            get
            {
                return new [] { "error" }.Concat(
                    Enumerable.Range(1, FeatureCount).Select(i => "lpc" + i)).ToArray();
            }
        }
            
        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        private readonly int _order;

        /// <summary>
        /// Length of analysis window (in ms)
        /// </summary>
        private readonly double _windowSize;

        /// <summary>
        /// Hop length (in ms)
        /// </summary>
        private readonly double _hopSize;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly double _preEmphasis;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="order"></param>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpcExtractor(int order, 
                            double windowSize = 0.0256, double hopSize = 0.010,
                            double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            _order = order;

            _window = window;
            _windowSize = windowSize;
            _hopSize = hopSize;

            _preEmphasis = preEmphasis;
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
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of LPC vectors</returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            // ====================================== PREPARE =======================================

            var hopSize = (int)(signal.SamplingRate * _hopSize);
            var windowSize = (int)(signal.SamplingRate * _windowSize);
            var windowSamples = Window.OfType(_window, windowSize);
            var fftSize = MathUtils.NextPowerOfTwo(2 * windowSize - 1);


            var blockReal = new double[fftSize];       // buffer for real parts of the currently processed block
            var blockImag = new double[fftSize];       // buffer for imaginary parts of the currently processed block
            var reversedReal = new double[fftSize];    // buffer for real parts of currently processed reversed block
            var reversedImag = new double[fftSize];    // buffer for imaginary parts of currently processed reversed block
            var zeroblock = new double[fftSize];       // just a buffer of zeros for quick memset

            var cc = new double[windowSize];           // buffer for (truncated) cross-correlation signal


            // 0) pre-emphasis (if needed)

            if (_preEmphasis > 0.0)
            {
                var preemphasisFilter = new PreEmphasisFilter(_preEmphasis);
                signal = preemphasisFilter.ApplyTo(signal);
            }


            // ================================= MAIN PROCESSING ==================================

            var featureVectors = new List<FeatureVector>();

            var i = startSample;
            while (i + windowSize < endSample)
            {
                // prepare all blocks in memory for the current step:

                FastCopy.ToExistingArray(zeroblock, blockReal, fftSize);
                FastCopy.ToExistingArray(zeroblock, blockImag, fftSize);
                FastCopy.ToExistingArray(zeroblock, reversedReal, fftSize);
                FastCopy.ToExistingArray(zeroblock, reversedImag, fftSize);

                FastCopy.ToExistingArray(signal.Samples, blockReal, windowSize, i);

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    blockReal.ApplyWindow(windowSamples);
                }

                // 2) autocorrelation

                Operation.CrossCorrelate(blockReal, blockImag, reversedReal, reversedImag, cc, windowSize);

                // 3) levinson-durbin

                var a = new double[_order + 1];
                var err = LevinsonDurbin(cc, a, _order);
                a[0] = err;

                // add LPC vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = a,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += hopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// Levinson-Durbin algorithm for solving main LPC task
        /// </summary>
        /// <param name="input">Auto-correlation vector</param>
        /// <param name="a">LP coefficients</param>
        /// <param name="order">Order of LPC</param>
        /// <returns>Prediction error</returns>
        public static double LevinsonDurbin(double[] input, double[] a, int order)
        {
            var err = input[0];

            a[0] = 1.0;

            for (var i = 1; i <= order; i++)
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
        /// Method returns LPC order for a given sampling rate 
        /// according to the best practices.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <returns>LPC order</returns>
        public static int EstimateOrder(int samplingRate)
        {
            return 2 + samplingRate / 1000;
        }
    }
}
