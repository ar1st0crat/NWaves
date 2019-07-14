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
        /// Number of features coincides with the order of LPC
        /// </summary>
        public override int FeatureCount => _order;

        /// <summary>
        /// Descriptions ("error", "lpc1", "lpc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions => 
            new[] { "error" }.Concat(
                    Enumerable.Range(1, FeatureCount).Select(i => "lpc" + i)).ToList();

        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        private readonly int _order;

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

            : base(samplingRate, frameDuration, hopDuration, preEmphasis)
        {
            _order = order;

            _blockSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_blockSize);

            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

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
        /// <returns>LPC vector</returns>
        public override float[] ProcessFrame(float[] block)
        {
            // 1) apply window (usually signal isn't windowed for LPC, so we check first)

            if (_window != WindowTypes.Rectangular)
            {
                block.ApplyWindow(_windowSamples);
            }

            block.FastCopyTo(_reversed, FrameSize);

            // 2) autocorrelation

            _convolver.CrossCorrelate(block, _reversed, _cc);

            // 3) levinson-durbin

            var lpc = new float[_order + 1];
            var err = Lpc.LevinsonDurbin(_cc, lpc, _order, FrameSize - 1);
            lpc[0] = err;

            return lpc;
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
