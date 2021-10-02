using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Interface for feature extractors that support parallelized computations.
    /// </summary>
    public interface IParallelFeatureExtractor
    {
        /// <summary>
        /// <para>Computes parallelly the feature vectors from <paramref name="samples"/>.</para>
        /// <para>Returns the list of computed feature vectors or empty list, if the number of samples is less than the size of analysis frame.</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="parallelThreads">Number of threads</param>
        List<float[]> ParallelComputeFrom(float[] samples, int parallelThreads);

        /// <summary>
        /// <para>Computes parallelly the feature vectors from <paramref name="samples"/>.</para>
        /// <para>Returns the list of computed feature vectors or empty list, if the number of samples is less than the size of analysis frame.</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="startSample">Index of the first sample in array for processing</param>
        /// <param name="endSample">Index of the last sample in array for processing</param>
        /// <param name="parallelThreads">Number of threads</param>
        List<float[]> ParallelComputeFrom(float[] samples, int startSample, int endSample, int parallelThreads);
    }
}
