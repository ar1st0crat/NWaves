using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Operations.Convolution;
using NWaves.Utils;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Linear Predictive Coding coefficients extractor
    /// </summary>
    public class LpcExtractor : FeatureExtractor
    {
        /// <summary>
        /// Descriptions ("error", "lpc1", "lpc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions => 
            new[] { "error" }.Concat(Enumerable.Range(1, _order).Select(i => "lpc" + i)).ToList();

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
        /// <param name="options">LPC options</param>
        public LpcExtractor(LpcOptions options) : base(options)
        {
            _order = options.LpcOrder;

            FeatureCount = _order + 1;

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
            new LpcExtractor(new LpcOptions
            {
                SamplingRate = SamplingRate,
                LpcOrder = _order,
                FrameDuration = FrameDuration,
                HopDuration = HopDuration,
                PreEmphasis = _preEmphasis,
                Window = _window
            });
    }
}
