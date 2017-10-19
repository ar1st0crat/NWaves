using System.Collections.Generic;
using NWaves.Signals;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// General interface for all feature extractors
    /// </summary>
    public interface IFeatureExtractor
    {
        /// <summary>
        /// String annotations (or simply names) of features
        /// </summary>
        IEnumerable<string> FeatureDescriptions { get; }

        /// <summary>
        /// Number of features to extract
        /// </summary>
        int FeatureCount { get; }

        /// <summary>
        /// Compute sequence of feature vectors from custom sequence of samples
        /// </summary>
        /// <param name="samples">Sequence of real-valued samples</param>
        /// <returns>Sequence of feature vectors</returns>
        IEnumerable<FeatureVector> ComputeFrom(IEnumerable<double> samples);

        /// <summary>
        /// Compute sequence of feature vectors from the DiscreteSignal object
        /// </summary>
        /// <param name="signal">Discrete real-valued signal</param>
        /// <returns>Sequence of feature vectors</returns>
        IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal);

        /// <summary>
        /// Compute sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Sample number of fragment's beginning</param>
        /// <param name="endPos">Sample number of fragment's end</param>
        /// <returns>Sequence of feature vectors</returns>
        IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal, int startPos, int endPos);
    }
}
