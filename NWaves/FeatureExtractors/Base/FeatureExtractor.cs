using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public List<FeatureVector> ComputeFrom(IEnumerable<float> samples, int samplingRate)
        {
            return ComputeFrom(new DiscreteSignal(samplingRate, samples));
        }

        /// <summary>
        /// Parallel computation (returns chunks of fecture vector lists)
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public virtual List<FeatureVector>[] ParallelChunksComputeFrom(DiscreteSignal signal)
        {
            var threadCount = Environment.ProcessorCount;
            var chunkSize = signal.Length / threadCount;

            var featureVectors = new List<FeatureVector>[threadCount];

            Parallel.For(0, threadCount, i =>
            {
                var startSample = i * chunkSize;
                var endSample = (i < threadCount - 1) ? (i + 1) * chunkSize : signal.Length;
                featureVectors[i] = ComputeFrom(signal, startSample, endSample);
            });

            return featureVectors;
        }

        /// <summary>
        /// Parallel computation (joins chunks of feature vector lists into one list)
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public virtual List<FeatureVector> ParallelComputeFrom(DiscreteSignal signal)
        {
            var chunks = ParallelChunksComputeFrom(signal);
            var featureVectors = new List<FeatureVector>();

            foreach (var vectors in chunks)
            {
                featureVectors.AddRange(vectors);
            }

            return featureVectors;
        }
    }
}
