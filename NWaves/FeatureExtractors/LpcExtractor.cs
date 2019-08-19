using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Operations.Convolution;
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
        /// Number of features equals to order of LPC + 1
        /// </summary>
        public override int FeatureCount => _order + 1;

        /// <summary>
        /// Descriptions ("error", "lpc1", "lpc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions => 
            new[] { "error" }.Concat(
                    Enumerable.Range(1, FeatureCount).Select(i => "lpc" + i)).ToList();

        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        protected readonly int _order;

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
        /// <param name="order"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpcExtractor(int samplingRate, 
                            int order, 
                            double frameDuration = 0.0256/*sec*/,
                            double hopDuration = 0.010/*sec*/,
                            double preEmphasis = 0,
                            WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration, preEmphasis, window)
        {
            _order = order;

            _blockSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_blockSize);

            _reversed = new float[FrameSize];
            _cc = new float[_blockSize];
        }

        /// <summary>
        /// Standard method for computing LPC vector.
        ///  
        /// Note:
        ///     The first LP coefficient is always equal to 1.0.
        ///     This method replaces it with the value of prediction error.
        /// 
        /// </summary>
        /// <param name="block">Samples for analysis</param>
        /// <param name="features">LPC vector</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            block.FastCopyTo(_reversed, FrameSize);

            // 1) autocorrelation

            _convolver.CrossCorrelate(block, _reversed, _cc);

            // 2) levinson-durbin

            var err = Lpc.LevinsonDurbin(_cc, features, _order, FrameSize - 1);

            features[0] = err;
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
