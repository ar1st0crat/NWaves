using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Transforms.Wavelets;
using NWaves.Utils;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Wavelet extractor
    /// </summary>
    public class WaveletExtractor : FeatureExtractor
    {
        /// <summary>
        /// Descriptions (simply "w0", "w1", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "w" + i).ToList();

        /// <summary>
        /// Fast Wavelet Transformer
        /// </summary>
        protected readonly Fwt _fwt;

        /// <summary>
        /// Wavelet name
        /// </summary>
        protected readonly string _waveletName;

        /// <summary>
        /// FWT level (0 = auto)
        /// </summary>
        protected readonly int _level;

        /// <summary>
        /// Internal buffer for FWT coefficients
        /// </summary>
        protected readonly float[] _coeffs;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Wavelet options</param>
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
        /// Compute FWT coeffs in each frame
        /// </summary>
        /// <param name="block"></param>
        /// <param name="features"></param>
        /// <returns></returns>
        public override void ProcessFrame(float[] block, float[] features)
        {
            _fwt.Direct(block, _coeffs, _level);

            _coeffs.FastCopyTo(features, FeatureCount);
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
