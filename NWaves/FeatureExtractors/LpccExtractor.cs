using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Operations.Convolution;
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
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "lpcc" + i).ToList();

        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        private readonly int _order;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        private readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients
        /// </summary>
        private readonly float[] _lifterCoeffs;

        /// <summary>
        /// FFT size
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Internal convolver
        /// </summary>
        private readonly Convolver _convolver;

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
        private float[] _block;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block
        /// </summary>
        private float[] _reversed;

        /// <summary>
        /// Internal buffer for cross-correlation signal
        /// </summary>
        float[] _cc;

        /// <summary>
        /// Internal buffer for LPC-coefficients
        /// </summary>
        float[] _lpc;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="frameSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpccExtractor(int samplingRate,
                             int featureCount,
                             double frameDuration = 0.0256/*sec*/,
                             double hopDuration = 0.010/*sec*/,
                             int lifterSize = 22,
                             double preEmphasis = 0.0,
                             WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration)
        {
            FeatureCount = featureCount;

            _order = featureCount;

            _fftSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_fftSize);

            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _lifterSize = lifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _preEmphasis = (float)preEmphasis;

            _block = new float[FrameSize];
            _reversed = new float[FrameSize];
            _cc = new float[_fftSize];
            _lpc = new float[_order + 1];
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
            Guard.AgainstInequality(SamplingRate, signal.SamplingRate, "Feature extractor sampling rate", "signal sampling rate");

            var hopSize = HopSize;
            var frameSize = FrameSize;

            var featureVectors = new List<FeatureVector>();

            var prevSample = startSample > 0 ? signal[startSample - 1] : 0.0f;

            var i = startSample;
            while (i + frameSize < endSample)
            {
                // prepare all blocks in memory for the current step:

                signal.Samples.FastCopyTo(_block, frameSize, i);
                signal.Samples.FastCopyTo(_reversed, frameSize, i);
                
                // 0) pre-emphasis (if needed)

                if (_preEmphasis > 1e-10)
                {
                    for (var k = 0; k < frameSize; k++)
                    {
                        var y = _block[k] - prevSample * _preEmphasis;
                        prevSample = _block[k];
                        _block[k] = y;
                    }
                    prevSample = signal[i + hopSize - 1];
                }

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    _block.ApplyWindow(_windowSamples);
                }

                // 2) autocorrelation

                _convolver.CrossCorrelate(_block, _reversed, _cc);

                // 3) Levinson-Durbin

                for (int k = 0; k < _lpc.Length; k++) _lpc[k] = 0;

                var err = MathUtils.LevinsonDurbin(_cc, _lpc, _order, frameSize - 1);

                // 4) simple and efficient algorithm for obtaining LPCC coefficients from LPC

                var lpcc = new float[FeatureCount];

                lpcc[0] = (float)Math.Log(err);

                for (var n = 1; n < FeatureCount; n++)
                {
                    var acc = 0.0f;
                    for (var k = 1; k < n; k++)
                    {
                        acc += k * lpcc[k] * _lpc[n - k];
                    }
                    lpcc[n] = -_lpc[n] - acc / n;
                }

                // (optional) liftering

                if (_lifterCoeffs != null)
                {
                    lpcc.ApplyWindow(_lifterCoeffs);
                }


                // add LPC vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = lpcc,
                    TimePosition = (double)i / SamplingRate
                });

                i += hopSize;
            }

            return featureVectors;
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
            new LpccExtractor(SamplingRate, FeatureCount, FrameDuration, HopDuration, _lifterSize, _preEmphasis, _window);
    }
}
