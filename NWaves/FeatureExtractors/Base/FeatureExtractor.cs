using System.Collections.Generic;
using NWaves.Signals;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Abstract class for all feature extractors
    /// </summary>
    public abstract class FeatureExtractor
    {
        /// <summary>
        /// Number of features to extract
        /// </summary>
        public abstract int FeatureCount { get; }

        /// <summary>
        /// String annotations (or simply names) of features
        /// </summary>
        public abstract IEnumerable<string> FeatureDescriptions { get; }
        
        /// <summary>
        /// Compute the sequence of feature vectors from the DiscreteSignal object
        /// </summary>
        /// <param name="signal">Discrete real-valued signal</param>
        /// <returns>Sequence of feature vectors</returns>
        public abstract IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal);

        /// <summary>
        /// Compute the sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Sample number of fragment's beginning</param>
        /// <param name="endPos">Sample number of fragment's end</param>
        /// <returns>Sequence of feature vectors</returns>
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal, int startPos, int endPos)
        {
            return ComputeFrom(signal[startPos, endPos]);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from custom sequence of samples
        /// </summary>
        /// <param name="samples">Sequence of real-valued samples</param>
        /// <returns>Sequence of feature vectors</returns>
        public IEnumerable<FeatureVector> ComputeFrom(IEnumerable<double> samples)
        {
            return ComputeFrom(new DiscreteSignal(1, samples));
        }
    }
}
