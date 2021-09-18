using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Linear Prediction Cepstral Coefficients (LPCC) extractor
    /// </summary>
    public class LpccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Feature names (simply "lpcc0", "lpcc1", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "lpcc" + i).ToList();

        /// <summary>
        /// Order of an LPC-filter.
        /// </summary>
        protected readonly int _order;

        /// <summary>
        /// Size of liftering window.
        /// </summary>
        protected readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients.
        /// </summary>
        protected readonly float[] _lifterCoeffs;

        /// <summary>
        /// Internal convolver.
        /// </summary>
        protected readonly Convolver _convolver;

        /// <summary>
        /// Internal buffer for cross-correlation signal.
        /// </summary>
        protected readonly float[] _cc;

        /// <summary>
        /// Internal buffer for LPC-coefficients.
        /// </summary>
        protected readonly float[] _lpc;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block.
        /// </summary>
        protected readonly float[] _reversed;

        /// <summary>
        /// Construct extractor from configuration options.
        /// </summary>
        /// <param name="options">Extractor configuration options</param>
        public LpccExtractor(LpccOptions options) : base(options)
        {
            FeatureCount = options.FeatureCount;

            _order = options.LpcOrder > 0 ? options.LpcOrder : FeatureCount - 1;

            _blockSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_blockSize);

            _lifterSize = options.LifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _reversed = new float[FrameSize];
            _cc = new float[_blockSize];
            _lpc = new float[_order + 1];
        }

        /// <summary>
        /// Compute LPCC vector in one frame.
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one LPCC feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            // The code here essentially duplicates LPC extractor code 
            // (for efficient memory usage it doesn't just delegate its work to LpcExtractor)
            // and then post-processes LPC vectors to obtain LPCC coefficients.
             
            block.FastCopyTo(_reversed, FrameSize);

            // 1) autocorrelation

            _convolver.CrossCorrelate(block, _reversed, _cc);

            // 2) Levinson-Durbin

            for (int k = 0; k < _lpc.Length; _lpc[k] = 0, k++) ;

            var err = Lpc.LevinsonDurbin(_cc, _lpc, _order, FrameSize - 1);

            // 3) compute LPCC coefficients from LPC

            Lpc.ToCepstrum(_lpc, err, features);

            // 4) (optional) liftering

            if (_lifterCoeffs != null)
            {
                features.ApplyWindow(_lifterCoeffs);
            }
        }

        /// <summary>
        /// Does the extractor support parallelization. Returns true always.
        /// </summary>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Thread-safe copy of the extractor for parallel computations.
        /// </summary>
        public override FeatureExtractor ParallelCopy() =>
            new LpccExtractor(new LpccOptions
            {
                SamplingRate = SamplingRate,
                FeatureCount = FeatureCount,
                FrameDuration = FrameDuration,
                HopDuration = HopDuration,
                LpcOrder = _order,
                LifterSize = _lifterSize,
                PreEmphasis = _preEmphasis,
                Window = _window
            });
    }
}
