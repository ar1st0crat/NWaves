using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Signals;
using NWaves.Transforms.Windows;

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
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public LpccExtractor(int featureCount, int windowSize = 512, int hopSize = 256,
                            double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            FeatureCount = featureCount;

            _lpcExtractor = new LpcExtractor(featureCount, windowSize, hopSize, preEmphasis, window);
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

            lpcc[0] = Math.Log(Math.Sqrt(lpc[0]));
            
            for (var n = 1; n < FeatureCount; n++)
            {
                var acc = lpc[n];
                for (var k = 1; k < n; k++)
                {
                    acc += k * lpcc[k] * lpc[n - k];
                }
                lpcc[n] = acc / n;
            }

            return new FeatureVector
            {
                Features = lpcc,
                TimePosition = lp.TimePosition
            };
        }
    }
}
