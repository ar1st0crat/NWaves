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
        /// Number of features (currently it's 1 pitch value estimated by autocorrelation)
        /// </summary>
        public override int FeatureCount => 1;

        /// <summary>
        /// Names of pitch algorithms
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Lower pitch frequency
        /// </summary>
        protected readonly float _low;

        /// <summary>
        /// Upper pitch frequency
        /// </summary>
        protected readonly float _high;

        /// <summary>
        /// Internal convolver
        /// </summary>
        protected readonly Convolver _convolver;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block
        /// </summary>
        protected readonly float[] _reversed;

        /// <summary>
        /// Internal buffer for cross-correlation signal
        /// </summary>
        protected readonly float[] _cc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public PitchExtractor(int samplingRate,
                              double frameDuration = 0.0256/*sec*/,
                              double hopDuration = 0.010/*sec*/,
                              float low = 80,
                              float high = 400,
                              double preEmphasis = 0,
                              WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration, preEmphasis, window)
        {
            _low = low;
            _high = high;

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
        /// <param name="features">Pitch</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            block.FastCopyTo(_reversed, FrameSize);

            // 1) autocorrelation

            _convolver.CrossCorrelate(block, _reversed, _cc);

            // 2) argmax of autocorrelation

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

            features[0] = max > 1.0f ? (float)SamplingRate / peakIndex : 0;
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
