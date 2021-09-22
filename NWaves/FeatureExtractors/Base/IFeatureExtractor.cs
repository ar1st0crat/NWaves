using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Interface for feature extractors.
    /// </summary>
    public interface IFeatureExtractor
    {
        /// <summary>
        /// Gets number of features to extract (feature vector size).
        /// </summary>
        int FeatureCount { get; }

        /// <summary>
        /// <para>Compute feature vectors from <paramref name="samples"/>.</para>
        /// <para>Returns the list of computed feature vectors or empty list, if the number of samples is less than the size of analysis frame.</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        List<float[]> ComputeFrom(float[] samples);

        /// <summary>
        /// <para>Compute feature vectors from <paramref name="samples"/>.</para>
        /// <para>Returns the list of computed feature vectors or empty list, if the number of samples is less than the size of analysis frame.</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="startSample">Index of the first sample in array for processing</param>
        /// <param name="endSample">Index of the last sample in array for processing</param>
        List<float[]> ComputeFrom(float[] samples, int startSample, int endSample);

        /// <summary>
        /// <para>Compute feature vectors from <paramref name="samples"/> and store them in <paramref name="vectors"/>.</para>
        /// <para>Returns the number of actually computed feature vectors</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="vectors">Pre-allocated sequence for storing the resulting feature vectors</param>
        int ComputeFrom(float[] samples, IList<float[]> vectors);

        /// <summary>
        /// <para>Compute feature vectors from <paramref name="samples"/> and store them in <paramref name="vectors"/>.</para>
        /// <para>Returns the number of actually computed feature vectors</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="startSample">Index of the first sample in array for processing</param>
        /// <param name="endSample">Index of the last sample in array for processing</param>
        /// <param name="vectors">Pre-allocated sequence for storing the resulting feature vectors</param>
        int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors);

        /// <summary>
        /// Reset feature extractor.
        /// </summary>
        void Reset();
    }
}
