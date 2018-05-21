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
        /// Length of analysis frame (in seconds)
        /// </summary>
        public double FrameSize { get; set; }

        /// <summary>
        /// Hop length (in seconds)
        /// </summary>
        public double HopSize { get; set; }

        /// <summary>
        /// Constructor requires FrameSize and HopSize to be set
        /// </summary>
        /// <param name="frameSize"></param>
        /// <param name="hopSize"></param>
        protected FeatureExtractor(double frameSize, double hopSize)
        {
            FrameSize = frameSize;
            HopSize = hopSize;
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
        /// <param name="parallelThreads"></param>
        /// <returns></returns>
        public virtual List<FeatureVector>[] ParallelChunksComputeFrom(DiscreteSignal signal, int parallelThreads = 0)
        {
            var threadCount = parallelThreads > 0 ? parallelThreads : Environment.ProcessorCount;
            var chunkSize = signal.Length / threadCount;
            
            // ============== carefully define the sample positions for merging ===============

            var startPositions = new int[threadCount];
            var endPositions = new int[threadCount];

            var frameSize = (int)(FrameSize * signal.SamplingRate);
            var hopSize = (int)(HopSize * signal.SamplingRate);
            var hopCount = (chunkSize - frameSize) / hopSize;

            var lastPosition = 0;
            for (var i = 0; i < threadCount; i++)
            {
                startPositions[i] = lastPosition;
                endPositions[i] = lastPosition + hopCount * hopSize + frameSize;
                lastPosition = endPositions[i] - frameSize;
            }

            endPositions[threadCount - 1] = signal.Length;

            // =========================== actual parallel computing ===========================

            var featureVectors = new List<FeatureVector>[threadCount];

            Parallel.For(0, threadCount, i =>
            {
                featureVectors[i] = ComputeFrom(signal, startPositions[i], endPositions[i]);
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
