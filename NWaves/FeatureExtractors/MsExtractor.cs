using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Signals;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Modulation spectra extractor
    /// </summary>
    public class MsExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients in modulation spectrum
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "mf0", "mf1", etc.)
        /// </summary>
        public override IEnumerable<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "mf" + i);
        
        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MsExtractor(int featureCount, int windowSize = 512, int hopSize = 256,
                           double preEmphasis = 0.0, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;
        }

        /// <summary>
        /// Method for computing modulation spectra.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            return new List<FeatureVector>();
        }
    }
}
