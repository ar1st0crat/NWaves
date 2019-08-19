using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Linear Prediction Cepstral Coefficients extractor
    /// </summary>
    public class LpccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of LPCC coefficients
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "lpcc0", "lpcc1", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "lpcc" + i).ToList();

        /// <summary>
        /// Order of an LPC-filter
        /// </summary>
        protected readonly int _order;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        protected readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients
        /// </summary>
        protected readonly float[] _lifterCoeffs;

        /// <summary>
        /// Internal convolver
        /// </summary>
        protected readonly Convolver _convolver;

        /// <summary>
        /// Internal buffer for cross-correlation signal
        /// </summary>
        protected readonly float[] _cc;

        /// <summary>
        /// Internal buffer for LPC-coefficients
        /// </summary>
        protected readonly float[] _lpc;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block
        /// </summary>
        protected readonly float[] _reversed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="frameSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpccExtractor(int samplingRate,
                             int featureCount,
                             double frameDuration = 0.0256/*sec*/,
                             double hopDuration = 0.010/*sec*/,
                             int lpcOrder = 0,
                             int lifterSize = 22,
                             double preEmphasis = 0,
                             WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration, preEmphasis, window)
        {
            FeatureCount = featureCount;

            _order = lpcOrder > 0 ? lpcOrder : featureCount - 1;

            _blockSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_blockSize);

            _lifterSize = lifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _reversed = new float[FrameSize];
            _cc = new float[_blockSize];
            _lpc = new float[_order + 1];
        }

        /// <summary>
        /// Method for computing LPCC features.
        /// It essentially duplicates LPC extractor code 
        /// (for efficient memory usage it doesn't just delegate its work to LpcExtractor)
        /// and then post-processes LPC vectors to obtain LPCC coefficients.
        /// </summary>
        /// <param name="block">Samples for analysis</param>
        /// <param name="features">LPCC vector</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
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
        /// True if computations can be done in parallel
        /// </summary>
        /// <returns></returns>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() =>
            new LpccExtractor(SamplingRate, FeatureCount, FrameDuration, HopDuration, _order, _lifterSize, _preEmphasis, _window);
    }
}
