using System;
using System.Collections.Generic;
using NWaves.FeatureExtractors.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Pitch extractor calls autocorrelation method since it's best in terms of universality and quality.
    /// The feature vector contains 1 component : pitch.
    /// 
    /// If there's a need to create pitch extractor based on other time-domain method (YIN or ZcrSchmitt),
    /// then TimeDomainFeatureExtractor can be used.
    /// 
    /// If there's a need to create pitch extractor based on a certain spectral method (HSS or HPS),
    /// then SpectralDomainFeatureExtractor can be used.
    /// 
    /// Example:
    /// 
    /// var extractor = new TimeDomainFeaturesExtractor(sr, "en", 0.0256, 0.010);
    /// 
    /// extractor.AddFeature("yin", (s, start, end) => { return Pitch.FromYin(s, start, end); });
    /// 
    /// var pitches = extractor.ComputeFrom(signal);
    /// 
    /// </summary>
    public class PitchExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of features (currently 1 pitch value estimated by autocorrelation)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Names of pitch algorithms
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Lower pitch frequency
        /// </summary>
        private readonly float _low;

        /// <summary>
        /// Upper pitch frequency
        /// </summary>
        private readonly float _high;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Window samples
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Internal convolver
        /// </summary>
        private readonly Convolver _convolver;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block
        /// </summary>
        private readonly float[] _reversed;

        /// <summary>
        /// Internal buffer for cross-correlation signal
        /// </summary>
        private readonly float[] _cc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="parameters"></param>
        public PitchExtractor(int samplingRate,
                              double frameDuration = 0.0256/*sec*/,
                              double hopDuration = 0.010/*sec*/,
                              float low = 80,
                              float high = 400,
                              double preEmphasis = 0,
                              WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration, preEmphasis)
        {
            _low = low;
            _high = high;

            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _blockSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_blockSize);

            _reversed = new float[FrameSize];
            _cc = new float[_blockSize];

            FeatureDescriptions = new List<string>() { "pitch" };
        }

        /// <summary>
        /// Pitch tracking
        /// </summary>
        /// <param name="block">Samples</param>
        /// <returns>Array of one element: pitch</returns>
        public override float[] ProcessFrame(float[] block)
        {
            // 1) apply window

            if (_window != WindowTypes.Rectangular)
            {
                block.ApplyWindow(_windowSamples);
            }

            block.FastCopyTo(_reversed, FrameSize);

            // 2) autocorrelation

            _convolver.CrossCorrelate(block, _reversed, _cc);

            // 3) argmax of autocorrelation

            var pitch1 = (int)(SamplingRate / _high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(SamplingRate / _low);     // 12,5 ms = 80Hz

            var start = pitch1 + FrameSize - 1;
            var end = Math.Min(start + pitch2, _cc.Length);

            var max = start < _cc.Length ? _cc[start] : 0;

            var peakIndex = start;
            for (var k = start; k < end; k++)
            {
                if (_cc[k] > max)
                {
                    max = _cc[k];
                    peakIndex = k - FrameSize + 1;
                }
            }

            var f0 = max > 1.0f ? (float)SamplingRate / peakIndex : 0;

            return new float[] { f0 };
        }

        /// <summary>
        /// Computations can be done in parallel
        /// </summary>
        /// <returns></returns>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() => 
            new PitchExtractor(SamplingRate, FrameDuration, HopDuration, _low, _high, _preEmphasis, _window);
    }
}
