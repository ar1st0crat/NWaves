using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Transforms.Wavelets;
using NWaves.Utils;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Represents wavelet extractor.
    /// </summary>
    public class WaveletExtractor : FeatureExtractor
    {
        /// <summary>
        /// Gets feature names (simply "w0", "w1", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "w" + i).ToList();

        /// <summary>
        /// Fast Wavelet Transformer.
        /// </summary>
        protected readonly Fwt _fwt;

        /// <summary>
        /// Wavelet name.
        /// </summary>
        protected readonly string _waveletName;

        /// <summary>
        /// FWT level (0 = auto).
        /// </summary>
        protected readonly int _level;

        /// <summary>
        /// Internal buffer for FWT coefficients.
        /// </summary>
        protected readonly float[] _coeffs;

        /// <summary>
        /// Constructs extractor from configuration <paramref name="options"/>.
        /// </summary>
        public WaveletExtractor(WaveletOptions options) : base(options)
        {
            _blockSize = options.FwtSize > FrameSize ? options.FwtSize : MathUtils.NextPowerOfTwo(FrameSize);

            FeatureCount = options.FeatureCount > 0 ? options.FeatureCount : _blockSize;

            _waveletName = options.WaveletName;
            _level = options.FwtLevel;
            _fwt = new Fwt(_blockSize, new Wavelet(_waveletName));
            
            _coeffs = new float[_blockSize];
        }

        /// <summary>
        /// Computes vector of FWT coefficients in one frame.
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one FWT feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            _fwt.Direct(block, _coeffs, _level);

            _coeffs.FastCopyTo(features, FeatureCount);
        }

        /// <summary>
        /// Returns true, since <see cref="WaveletExtractor"/> always supports parallelization.
        /// </summary>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Creates thread-safe copy of the extractor for parallel computations.
        /// </summary>
        public override FeatureExtractor ParallelCopy() =>
            new WaveletExtractor(
                new WaveletOptions
                {
                    SamplingRate = SamplingRate,
                    FrameDuration = FrameDuration,
                    HopDuration = HopDuration,
                    WaveletName = _waveletName,
                    FeatureCount = FeatureCount,
                    FwtSize = _blockSize,
                    FwtLevel = _level,
                    PreEmphasis = _preEmphasis,
                    Window = _window
                });
    }
}
