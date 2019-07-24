using NWaves.FeatureExtractors.Base;
using NWaves.Transforms.Wavelets;
using NWaves.Utils;
using NWaves.Windows;
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
        /// Number of FWT coefficients
        /// </summary>
        public override int FeatureCount => _coeffCount;

        /// <summary>
        /// Descriptions (simply "w0", "w1", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "w" + i).ToList();

        /// <summary>
        /// Number of wavelet coefficients to keep in feature vector
        /// </summary>
        protected readonly int _coeffCount;

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
        /// <param name="samplingRate"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="waveletName"></param>
        /// <param name="fwtSize"></param>
        /// <param name="fwtLevel"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public WaveletExtractor(int samplingRate,
                                double frameDuration,
                                double hopDuration,
                                string waveletName,
                                int coeffCount = 0,
                                int fwtSize = 0,
                                int fwtLevel = 0,
                                double preEmphasis = 0,
                                WindowTypes window = WindowTypes.Rectangular)
            
            : base(samplingRate, frameDuration, hopDuration, preEmphasis, window)
        {
            _blockSize = fwtSize > FrameSize ? fwtSize : MathUtils.NextPowerOfTwo(FrameSize);

            _fwt = new Fwt(_blockSize, new Wavelet(waveletName));

            _waveletName = waveletName;
            _level = fwtLevel;

            _coeffCount = coeffCount > 0 ? coeffCount : _blockSize;
            _coeffs = new float[_blockSize];
        }

        /// <summary>
        /// Compute FWT coeffs in each frame
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public override float[] ProcessFrame(float[] block)
        {
            _fwt.Direct(block, _coeffs, _level);

            var coeffs = new float[_coeffCount];
            _coeffs.FastCopyTo(coeffs, coeffs.Length);

            return coeffs;
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
            new WaveletExtractor(SamplingRate, FrameDuration, HopDuration, _waveletName, _coeffCount, _blockSize, _level, _preEmphasis, _window);
    }
}
