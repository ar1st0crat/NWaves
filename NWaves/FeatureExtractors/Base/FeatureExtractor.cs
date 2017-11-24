using System.Collections.Generic;
using System.Linq;
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
        public abstract string[] FeatureDescriptions { get; }

        /// <summary>
        /// String annotations (or simply names) of delta features (1st order derivatives)
        /// </summary>
        public virtual string[] DeltaFeatureDescriptions
        {
            get { return FeatureDescriptions.Select(d => "delta_" + d).ToArray(); }
        }

        /// <summary>
        /// String annotations (or simply names) of delta-delta features (2nd order derivatives)
        /// </summary>
        public virtual string[] DeltaDeltaFeatureDescriptions
        {
            get { return FeatureDescriptions.Select(d => "delta_delta_" + d).ToArray(); }
        }
        
        /// <summary>
        /// Compute the sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>Sequence of feature vectors</returns>
        public abstract List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample);

        /// <summary>
        /// Compute the sequence of feature vectors from the entire DiscreteSignal
        /// </summary>
        /// <param name="signal">Discrete real-valued signal</param>
        /// <returns>Sequence of feature vectors</returns>
        public List<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            return ComputeFrom(signal, 0, signal.Length);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from custom sequence of samples
        /// </summary>
        /// <param name="samples">Sequence of real-valued samples</param>
        /// <param name="samplingRate">The sampling rate of the sequence</param>
        /// <returns>Sequence of feature vectors</returns>
        public List<FeatureVector> ComputeFrom(IEnumerable<double> samples, int samplingRate)
        {
            return ComputeFrom(new DiscreteSignal(samplingRate, samples));
        }
    }
}
