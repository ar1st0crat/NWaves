using System.Collections.Generic;
using NWaves.Signals;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFeatureExtractor
    {
        /// <summary>
        /// 
        /// </summary>
        IEnumerable<string> FeatureDescriptions { get; }

        /// <summary>
        /// 
        /// </summary>
        int FeatureCount { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal, int startPos, int endPos);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        IEnumerable<FeatureVector> ComputeFrom(IEnumerable<double> samples);
    }
}
