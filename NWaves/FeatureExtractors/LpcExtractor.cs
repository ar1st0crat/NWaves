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
        public override List<string> FeatureDescriptions => 
            new[] { "error" }.Concat(
                    Enumerable.Range(1, FeatureCount).Select(i => "lpc" + i)).ToList();

        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        private readonly int _order;

        /// <summary>
        /// FFT size
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Window samples
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly float _preEmphasis;

        /// <summary>
        /// Internal buffer for real parts of the currently processed block
        /// </summary>
        private float[] _blockReal;

        /// <summary>
        /// Internal buffer for imaginary parts of the currently processed block
        /// </summary>
        private float[] _blockImag;

        /// <summary>
        /// Internal buffer for real parts of currently processed reversed block
        /// </summary>
        private float[] _reversedReal;

        /// <summary>
        /// Internal buffer for imaginary parts of currently processed reversed block
        /// </summary>
        private float[] _reversedImag;

        /// <summary>
        /// Internal buffer of zeros for quick memset
        /// </summary>
        private readonly float[] _zeroblock;

        /// <summary>
        /// Internal buffer for (truncated) cross-correlation signal
        /// </summary>
        private float[] _cc;


        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="order"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpcExtractor(int samplingRate, 
                            int order, 
                            double frameDuration = 0.0256/*sec*/,
                            double hopDuration = 0.010/*sec*/,
                            double preEmphasis = 0.0,
                            WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration)
        {
            _order = order;

            _fftSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);

            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _preEmphasis = (float) preEmphasis;

            _blockReal = new float[_fftSize];
            _blockImag = new float[_fftSize];
            _reversedReal = new float[_fftSize];
            _reversedImag = new float[_fftSize];
            _zeroblock = new float[_fftSize];
            _cc = new float[FrameSize];
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
            Guard.AgainstInequality(SamplingRate, signal.SamplingRate, "Feature extractor sampling rate", "signal sampling rate");

            var frameSize = FrameSize;
            var hopSize = HopSize;

            var featureVectors = new List<FeatureVector>();

            var prevSample = startSample > 0 ? signal[startSample - 1] : 0.0f;

            var i = startSample;
            while (i + frameSize < endSample)
            {
                // prepare all blocks in memory for the current step:

                _zeroblock.FastCopyTo(_blockReal, _fftSize);
                _zeroblock.FastCopyTo(_blockImag, _fftSize);
                _zeroblock.FastCopyTo(_reversedReal, _fftSize);
                _zeroblock.FastCopyTo(_reversedImag, _fftSize);

                signal.Samples.FastCopyTo(_blockReal, frameSize, i);


                // 0) pre-emphasis (if needed)

                if (_preEmphasis > 1e-10)
                {
                    for (var k = 0; k < frameSize; k++)
                    {
                        var y = _blockReal[k] - prevSample * _preEmphasis;
                        prevSample = _blockReal[k];
                        _blockReal[k] = y;
                    }
                    prevSample = signal[i + hopSize - 1];
                }


                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    _blockReal.ApplyWindow(_windowSamples);
                }

                // 2) autocorrelation

                Operation.CrossCorrelate(_blockReal, _blockImag, _reversedReal, _reversedImag, _cc, frameSize);

                // 3) levinson-durbin

                var a = new float[_order + 1];
                var err = MathUtils.LevinsonDurbin(_cc, a, _order);
                a[0] = err;

                // add LPC vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = a,
                    TimePosition = (double) i / SamplingRate
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

        /// <summary>
        /// True if computations can be done in parallel
        /// </summary>
        /// <returns></returns>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() => 
            new LpcExtractor(SamplingRate, _order, FrameDuration, HopDuration, _preEmphasis, _window);
    }
}
