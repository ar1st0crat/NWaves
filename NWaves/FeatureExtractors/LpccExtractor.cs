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
        /// Coefficients of the liftering window
        /// </summary>
        private readonly double[] _lifterCoeffs;

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
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="samplingRate"></param>
        /// <param name="windowSize"></param>
        /// <param name="overlapSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpccExtractor(int featureCount, int samplingRate,
                             double windowSize = 0.0256, double overlapSize = 0.010, int lifterSize = 22,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            FeatureCount = featureCount;
            _lifterCoeffs = Window.Liftering(featureCount, lifterSize);

            _order = featureCount;

            _windowSize = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, _windowSize);
            _window = window;

            _hopSize = (int)(samplingRate * overlapSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            _fftSize = MathUtils.NextPowerOfTwo(2 * _windowSize - 1);
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
            var featureVectors = new List<FeatureVector>();

            var blockReal = new double[_fftSize];       // buffer for real parts of the currently processed block
            var blockImag = new double[_fftSize];       // buffer for imaginary parts of the currently processed block
            var reversedReal = new double[_fftSize];    // buffer for real parts of currently processed reversed block
            var reversedImag = new double[_fftSize];    // buffer for imaginary parts of currently processed reversed block
            var zeroblock = new double[_fftSize];       // just a buffer of zeros for quick memset

            var cc = new double[_windowSize];           // buffer for (truncated) cross-correlation signal
            var lpc = new double[_order + 1];           // buffer for LPC coefficients


            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;

            var i = startSample;
            while (i + _windowSize < endSample)
            {
                // prepare all blocks in memory for the current step:

                FastCopy.ToExistingArray(zeroblock, blockReal, _fftSize);
                FastCopy.ToExistingArray(zeroblock, blockImag, _fftSize);
                FastCopy.ToExistingArray(zeroblock, reversedReal, _fftSize);
                FastCopy.ToExistingArray(zeroblock, reversedImag, _fftSize);

                FastCopy.ToExistingArray(filtered.Samples, blockReal, _windowSize, i);

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    blockReal.ApplyWindow(_windowSamples);
                }

                // 2) autocorrelation

                Operation.CrossCorrelate(blockReal, blockImag, reversedReal, reversedImag, cc, _windowSize);

                // 3) levinson-durbin

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
                lpcc.ApplyWindow(_lifterCoeffs);


                // add LPC vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = lpcc,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _hopSize;
            }

            return featureVectors;
        }
    }
}
