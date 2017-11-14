using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Signals;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    public class LpccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of LPCC coefficients
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "lpcc0", "lpcc1", etc.)
        /// </summary>
        public override IEnumerable<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "lpcc" + i);

        /// <summary>
        /// Helper LPC extractor (basically, does all the heavy-lifting)
        /// </summary>
        private readonly LpcExtractor _lpcExtractor;

        /// <summary>
        /// Coefficients of the liftering window
        /// </summary>
        private readonly double[] _lifterCoeffs;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="samplingRate"></param>
        /// <param name="windowSize"></param>
        /// <param name="overlapSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpccExtractor(int featureCount, int samplingRate,
                             double windowSize = 0.0256, double overlapSize = 0.010, int lifterSize = 22,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            FeatureCount = featureCount;
            _lifterCoeffs = Window.Liftering(featureCount, lifterSize);

            _lpcExtractor = 
                new LpcExtractor(featureCount, samplingRate, windowSize, overlapSize, preEmphasis, window);
        }

        /// <summary>
        /// Method for computing LPCC features.
        /// It essentially delegates all the work to LPC extractor 
        /// and then post-processes LPC vectors to obtain LPCC coefficients.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var lpc = _lpcExtractor.ComputeFrom(signal);
            return lpc.Select(LpcToLpcc).ToList();
        }

        /// <summary>
        /// Simple algorithm for obtaining LPCC coefficients from LPC
        /// </summary>
        /// <param name="lp">LPC feature vector</param>
        /// <returns>LPCC feature vector</returns>
        public FeatureVector LpcToLpcc(FeatureVector lp)
        {
            var lpc = lp.Features;
            var lpcc = new double[FeatureCount];

            var gain = lpc[0];
            lpcc[0] = Math.Log(gain);

            for (var n = 1; n < FeatureCount; n++)
            {
                var acc = 0.0;
                for (var k = 1; k < n; k++)
                {
                    acc += k * lpcc[k] * lpc[n - k];
                }
                lpcc[n] = -lpc[n] - acc / n;
            }

            // (optional) liftering
            lpcc.ApplyWindow(_lifterCoeffs);

            return new FeatureVector
            {
                Features = lpcc,
                TimePosition = lp.TimePosition
            };
        }
    }
}
