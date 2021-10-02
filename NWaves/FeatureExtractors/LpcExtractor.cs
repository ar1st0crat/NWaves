using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Operations.Convolution;
using NWaves.Utils;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Represents Linear Predictive Coding (LPC) coefficients extractor.
    /// </summary>
    public class LpcExtractor : FeatureExtractor
    {
        /// <summary>
        /// Gets feature names ("error", "lpc1", "lpc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions => 
            new[] { "error" }.Concat(Enumerable.Range(1, _order).Select(i => "lpc" + i)).ToList();

        /// <summary>
        /// Order of an LPC-filter.
        /// </summary>
        protected readonly int _order;

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
        /// <para>Computes LPC vector in one frame.</para>
        /// <para>
        /// Note:
        ///     The first LP coefficient is always equal to 1.0. 
        ///     This method replaces it with the value of prediction error.
        /// </para>
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one LPC feature vector) computed in the block</param>
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
        /// Returns true, since <see cref="LpcExtractor"/> always supports parallelization.
        /// </summary>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Creates thread-safe copy of the extractor for parallel computations.
        /// </summary>
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
