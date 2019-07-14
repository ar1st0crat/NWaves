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
        private readonly int _order;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        private readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients
        /// </summary>
        private readonly float[] _lifterCoeffs;

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
        /// Internal buffer for cross-correlation signal
        /// </summary>
        private readonly float[] _cc;

        /// <summary>
        /// Internal buffer for LPC-coefficients
        /// </summary>
        private readonly float[] _lpc;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block
        /// </summary>
        private readonly float[] _reversed;

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

            : base(samplingRate, frameDuration, hopDuration, preEmphasis)
        {
            FeatureCount = featureCount;

            _order = lpcOrder > 0 ? lpcOrder : featureCount - 1;

            _blockSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(_blockSize);

            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

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
        /// <returns>LPCC vector</returns>
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

            // 3) Levinson-Durbin

            for (int k = 0; k < _lpc.Length; _lpc[k] = 0, k++) ;

            var err = Lpc.LevinsonDurbin(_cc, _lpc, _order, FrameSize - 1);

            // 4) compute LPCC coefficients from LPC

            var lpcc = new float[FeatureCount];

            Lpc.ToCepstrum(_lpc, err, lpcc);

            // 5) (optional) liftering

            if (_lifterCoeffs != null)
            {
                lpcc.ApplyWindow(_lifterCoeffs);
            }

            return lpcc;
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
