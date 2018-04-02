using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
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
        private readonly float _preEmphasis;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="frameSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpccExtractor(int featureCount,
                             double frameSize = 0.0256/*sec*/, double hopSize = 0.010/*sec*/, int lifterSize = 22,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
            : base(frameSize, hopSize)
        {
            FeatureCount = featureCount;
            _order = featureCount;
            _window = window;
            _lifterSize = lifterSize;
            _preEmphasis = (float)preEmphasis;
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

            var hopSize = (int)(signal.SamplingRate * HopSize);
            var frameSize = (int)(signal.SamplingRate * FrameSize);
            var windowSamples = Window.OfType(_window, frameSize);
            var fftSize = MathUtils.NextPowerOfTwo(2 * frameSize - 1);

            var lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;


            var blockReal = new float[fftSize];       // buffer for real parts of the currently processed block
            var blockImag = new float[fftSize];       // buffer for imaginary parts of the currently processed block
            var reversedReal = new float[fftSize];    // buffer for real parts of currently processed reversed block
            var reversedImag = new float[fftSize];    // buffer for imaginary parts of currently processed reversed block
            var zeroblock = new float[fftSize];       // just a buffer of zeros for quick memset

            var cc = new float[frameSize];           // buffer for (truncated) cross-correlation signal
            var lpc = new float[_order + 1];          // buffer for LPC coefficients


            // ================================= MAIN PROCESSING ==================================

            var featureVectors = new List<FeatureVector>();

            var prevSample = startSample > 0 ? signal[startSample - 1] : 0.0f;

            var i = startSample;
            while (i + frameSize < endSample)
            {
                // prepare all blocks in memory for the current step:

                zeroblock.FastCopyTo(blockReal, fftSize);
                zeroblock.FastCopyTo(blockImag, fftSize);
                zeroblock.FastCopyTo(reversedReal, fftSize);
                zeroblock.FastCopyTo(reversedImag, fftSize);

                signal.Samples.FastCopyTo(blockReal, frameSize, i);


                // 0) pre-emphasis (if needed)

                if (_preEmphasis > 0.0)
                {
                    for (var k = 0; k < frameSize; k++)
                    {
                        var y = blockReal[k] - prevSample * _preEmphasis;
                        prevSample = blockReal[k];
                        blockReal[k] = y;
                    }
                    prevSample = signal[i + hopSize - 1];
                }

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    blockReal.ApplyWindow(windowSamples);
                }

                // 2) autocorrelation

                Operation.CrossCorrelate(blockReal, blockImag, reversedReal, reversedImag, cc, frameSize);

                // 3) Levinson-Durbin

                zeroblock.FastCopyTo(lpc, lpc.Length);
                var err = MathUtils.LevinsonDurbin(cc, lpc, _order);

                // 4) simple and efficient algorithm for obtaining LPCC coefficients from LPC

                var lpcc = new float[FeatureCount];

                lpcc[0] = (float)Math.Log(err);

                for (var n = 1; n < FeatureCount; n++)
                {
                    var acc = 0.0f;
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
