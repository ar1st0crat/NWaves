using System;
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
    /// Linear Prediction Cepstral Coefficients extractor
    /// </summary>
    public class LpccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of LPCC coefficients
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "lpcc0", "lpcc1", etc.)
        /// </summary>
        public override string[] FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "lpcc" + i).ToArray();

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
        /// Size of liftering window
        /// </summary>
        private readonly int _lifterSize;

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
        /// <param name="featureCount"></param>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpccExtractor(int featureCount,
                             double windowSize = 0.0256, double hopSize = 0.010, int lifterSize = 22,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            FeatureCount = featureCount;

            _order = featureCount;

            _window = window;
            _windowSize = windowSize;
            _hopSize = hopSize;
            
            _lifterSize = lifterSize;
            _preEmphasis = preEmphasis;
        }

        /// <summary>
        /// Method for computing LPCC features.
        /// It essentially duplicates LPC extractor code 
        /// (for efficient memory usage it doesn't just delegate its work to LpcExtractor)
        /// and then post-processes LPC vectors to obtain LPCC coefficients.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns></returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            // ====================================== PREPARE =======================================

            var hopSize = (int)(signal.SamplingRate * _hopSize);
            var windowSize = (int)(signal.SamplingRate * _windowSize);
            var windowSamples = Window.OfType(_window, windowSize);
            var fftSize = MathUtils.NextPowerOfTwo(2 * windowSize - 1);

            var lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;


            var blockReal = new double[fftSize];       // buffer for real parts of the currently processed block
            var blockImag = new double[fftSize];       // buffer for imaginary parts of the currently processed block
            var reversedReal = new double[fftSize];    // buffer for real parts of currently processed reversed block
            var reversedImag = new double[fftSize];    // buffer for imaginary parts of currently processed reversed block
            var zeroblock = new double[fftSize];       // just a buffer of zeros for quick memset

            var cc = new double[windowSize];           // buffer for (truncated) cross-correlation signal
            var lpc = new double[_order + 1];          // buffer for LPC coefficients


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

                // 3) Levinson-Durbin

                FastCopy.ToExistingArray(zeroblock, lpc, lpc.Length);
                var err = LpcExtractor.LevinsonDurbin(cc, lpc, _order);

                // 4) simple and efficient algorithm for obtaining LPCC coefficients from LPC

                var lpcc = new double[FeatureCount];

                lpcc[0] = Math.Log(err);

                for (var n = 1; n < FeatureCount; n++)
                {
                    var acc = 0.0;
                    for (var k = 1; k < n; k++)
                    {
                        acc += k * lpcc[k] * lpc[n - k];
                    }
                    lpcc[n] = -lpc[n] - acc / n;
                }

                // (optional) liftering

                if (lifterCoeffs != null)
                {
                    lpcc.ApplyWindow(lifterCoeffs);
                }


                // add LPC vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = lpcc,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += hopSize;
            }

            return featureVectors;
        }
    }
}
