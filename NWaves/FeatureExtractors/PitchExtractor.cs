using System;
using System.Collections.Generic;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Multi;
using NWaves.FeatureExtractors.Options;
using NWaves.Operations.Convolution;
using NWaves.Utils;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Represents pitch extractor / tracker.
    /// <para>
    /// Pitch extractor calls autocorrelation method since it's best in terms of universality and quality. 
    /// The feature vector contains 1 component : pitch.
    /// </para>
    /// <para>
    /// If there's a need to create pitch extractor based on other time-domain method (YIN or ZcrSchmitt), 
    /// then <see cref="TimeDomainFeaturesExtractor"/> can be used.
    /// </para>
    /// <para>
    /// If there's a need to create pitch extractor based on a certain spectral method (HSS or HPS), 
    /// then <see cref="SpectralFeaturesExtractor"/> can be used.
    /// </para>
    /// <para>
    /// Example:
    /// 
    /// <code>
    /// var extractor = new TimeDomainFeaturesExtractor(sr, "en", 0.0256, 0.010);
    /// <br/>
    /// extractor.AddFeature("yin", (s, start, end) => { return Pitch.FromYin(s, start, end); });
    /// <br/>
    /// var pitches = extractor.ComputeFrom(signal);
    /// </code>
    /// </para>
    /// </summary>
    public class PitchExtractor : FeatureExtractor
    {
        /// <summary>
        /// Gets names of pitch estimation algorithms.
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Lower frequency of expected pitch range.
        /// </summary>
        protected readonly float _low;

        /// <summary>
        /// Upper frequency of expected pitch range.
        /// </summary>
        protected readonly float _high;

        /// <summary>
        /// Internal convolver.
        /// </summary>
        protected readonly Convolver _convolver;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block.
        /// </summary>
        protected readonly float[] _reversed;

        /// <summary>
        /// Internal buffer for cross-correlation signal.
        /// </summary>
        protected readonly float[] _cc;

        /// <summary>
        /// Constructs extractor from configuration <paramref name="options"/>.
        /// </summary>
        public PitchExtractor(PitchOptions options) : base(options)
        {
            _low = (float)options.LowFrequency;
            _high = (float)options.HighFrequency;

            _blockSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_blockSize);

            _reversed = new float[FrameSize];
            _cc = new float[_blockSize];

            FeatureCount = 1;
            FeatureDescriptions = new List<string>() { "pitch" };
        }

        /// <summary>
        /// Computes pitch in one frame.
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Pitch (feature vector containing only pitch) computed in the block</param>
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
        /// Returns true, since <see cref="PitchExtractor"/> always supports parallelization.
        /// </summary>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Creates thread-safe copy of the extractor for parallel computations.
        /// </summary>
        public override FeatureExtractor ParallelCopy() => 
            new PitchExtractor(new PitchOptions
            {
                SamplingRate = SamplingRate,
                FrameDuration = FrameDuration,
                HopDuration = HopDuration,
                LowFrequency = _low,
                HighFrequency = _high,
                PreEmphasis = _preEmphasis,
                Window = _window
            });
    }
}
