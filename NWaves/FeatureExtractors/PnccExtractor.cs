using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.FeatureExtractors.Base;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// 
    /// </summary>
    public class PnccExtractor : FeatureExtractor
    {
        /// <summary>
        /// 
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// 
        /// </summary>
        public override IEnumerable<string> FeatureDescriptions => 
            Enumerable.Range(0, FeatureCount).Select(i => "pncc" + i);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureCount"></param>
        public PnccExtractor(int featureCount = 12)
        {
            FeatureCount = featureCount;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            return null;
        }
    }
}
