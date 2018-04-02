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
        /// <param name="order"></param>
        /// <param name="frameSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpcExtractor(int order, 
                            double frameSize = 0.0256/*sec*/, double hopSize = 0.010/*sec*/,
                            double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
            : base(frameSize, hopSize)
        {
            _order = order;
            _window = window;
            _preEmphasis = (float)(preEmphasis);
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

            var hopSize = (int)(signal.SamplingRate * HopSize);
            var frameSize = (int)(signal.SamplingRate * FrameSize);
            var windowSamples = Window.OfType(_window, frameSize);
            var fftSize = MathUtils.NextPowerOfTwo(2 * frameSize - 1);

            var blockReal = new float[fftSize];       // buffer for real parts of the currently processed block
            var blockImag = new float[fftSize];       // buffer for imaginary parts of the currently processed block
            var reversedReal = new float[fftSize];    // buffer for real parts of currently processed reversed block
            var reversedImag = new float[fftSize];    // buffer for imaginary parts of currently processed reversed block
            var zeroblock = new float[fftSize];       // just a buffer of zeros for quick memset

            var cc = new float[frameSize];           // buffer for (truncated) cross-correlation signal


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

                // 3) levinson-durbin

                var a = new float[_order + 1];
                var err = MathUtils.LevinsonDurbin(cc, a, _order);
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
