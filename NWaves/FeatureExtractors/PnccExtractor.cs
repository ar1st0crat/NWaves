using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.FeatureExtractors.Base;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// 
    /// </summary>
    public class PnccExtractor : IFeatureExtractor
    {
        /// <summary>
        /// 
        /// </summary>
        public int FeatureCount { get; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<string> FeatureDescriptions => 
            Enumerable.Range(0, FeatureCount).Select(i => "Coefficient pncc" + i);

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
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            return ComputeFrom(signal.Samples);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal, int startPos, int endPos)
        {
            return ComputeFrom(signal.Samples);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(IEnumerable<double> samples)
        {
            var vector = new FeatureVector
            {
                Features = Enumerable.Repeat(0.0, FeatureCount).ToArray()
            };

            return new[] {vector};
        }
    }
}
