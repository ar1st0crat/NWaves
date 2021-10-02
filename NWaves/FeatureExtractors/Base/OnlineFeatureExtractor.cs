using NWaves.Utils;
using System;
using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// <see cref="FeatureExtractor"/> adapter for online feature extraction.
    /// </summary>
    public class OnlineFeatureExtractor : IFeatureExtractor
    {
        /// <summary>
        /// Gets or sets underlying feature extractor.
        /// </summary>
        public FeatureExtractor Extractor { get; set; }

        /// <summary>
        /// Gets number of features to extract (feature vector size).
        /// </summary>
        public int FeatureCount => Extractor.FeatureCount;

        /// <summary>
        /// Should the last non-processed samples in the current block be ignored in the next block.
        /// </summary>
        private readonly bool _ignoreLastSamples;

        /// <summary>
        /// The number of last non-processed samples in the current block that will be processed in the next block.
        /// </summary>
        private int _skippedCount = 0;

        /// <summary>
        /// Internal buffer for accumulated samples.
        /// </summary>
        private float[] _tempBuffer;

        /// <summary>
        /// Constructs <see cref="OnlineFeatureExtractor"/> as a wrapper around <paramref name="extractor"/>.
        /// </summary>
        /// <param name="extractor">Underlying feature extractor</param>
        /// <param name="ignoreLastSamples">Should the last non-processed samples in the current block be ignored in the next block</param>
        /// <param name="maxDataSize">Reserved max size of the internal buffer for accumulated samples</param>
        public OnlineFeatureExtractor(FeatureExtractor extractor, bool ignoreLastSamples = false, int maxDataSize = 0)
        {
            Extractor = extractor;
            _ignoreLastSamples = ignoreLastSamples;
            
            _tempBuffer = maxDataSize > 0 ? (new float[maxDataSize]) : (new float[extractor.SamplingRate]);
        }

        /// <summary>
        /// <para>Returns maximally possible number of output feature vectors (based on maximally possible online data portion size).</para>
        /// <para>This number is intended to be used for pre-allocation of feature vector lists.</para>
        /// </summary>
        /// <param name="dataSize">Maximally possible online data portion size (number of samples)</param>
        public int VectorCount(int dataSize)
        {
            return dataSize < Extractor.FrameSize ? 1 : dataSize / Extractor.HopSize + 1;
        }

        /// <summary>
        /// <para>Returns maximally possible number of output feature vectors (based on maximally possible duration of online data portion).</para>
        /// <para>This number is intended to be used for pre-allocation of feature vector lists.</para>
        /// </summary>
        /// <param name="seconds">Maximally possible duration of online data portion (in seconds)</param>
        public int VectorCountFromSeconds(double seconds) => VectorCount((int)(Extractor.SamplingRate * seconds));

        /// <summary>
        /// <para>Ensures the size of internal buffer for accumulated samples.</para>
        /// <para>If the new size exceeds the buffer size, it will be auto-resized.</para>
        /// </summary>
        /// <param name="dataSize">Required size (measured in sample count)</param>
        public void EnsureSize(int dataSize)
        {
            if (_tempBuffer.Length < dataSize)
            {
                Array.Resize(ref _tempBuffer, dataSize);
            }
        }

        /// <summary>
        /// <para>Ensures the size of internal buffer for accumulated samples based on required duration.</para>
        /// <para>If the new size (computed from duration) exceeds the buffer size, it will be auto-resized.</para>
        /// </summary>
        /// <param name="seconds">Required duration in seconds</param>
        public void EnsureSizeFromSeconds(double seconds)
        {
            var dataSize = (int)(seconds * Extractor.SamplingRate) + 1;

            if (_tempBuffer.Length < dataSize)
            {
                Array.Resize(ref _tempBuffer, dataSize);
            }
        }

        /// <summary>
        /// <para>Computes feature vectors from <paramref name="data"/> and stores them in <paramref name="featureVectors"/>.</para>
        /// <para>Returns the number of actually computed feature vectors.</para>
        /// </summary>
        /// <param name="data">Block of data</param>
        /// <param name="featureVectors">Pre-allocated sequence for storing the resulting feature vectors</param>
        public int ComputeFrom(float[] data, IList<float[]> featureVectors)
        {
            if (_ignoreLastSamples)
            {
                return Extractor.ComputeFrom(data, 0, data.Length, featureVectors);
            }

            // copy data to temp buffer;
            // don't touch first samples saved at previous iteration
            data.FastCopyTo(_tempBuffer, data.Length, 0, _skippedCount);

            var currentLength = data.Length + _skippedCount;

            var vectorCount = Extractor.ComputeFrom(_tempBuffer, 0, currentLength, featureVectors);

            // estimate number of non-processed samples:
            _skippedCount = currentLength - vectorCount * Extractor.HopSize;

            // and copy these last samples to the beginning of temp buffer:
            _tempBuffer.FastCopyTo(_tempBuffer, _skippedCount, currentLength - _skippedCount);

            return vectorCount;
        }

        /// <summary>
        /// <para>Computes feature vectors from <paramref name="data"/>.</para>
        /// <para>Returns the list of computed feature vectors or empty list, if the number of samples is less than the size of analysis frame.</para>
        /// </summary>
        /// <param name="data">Block of data</param>
        public List<float[]> ComputeFrom(float[] data)
        {
            var totalSize = data.Length + _skippedCount;
            var vectorCount = totalSize < Extractor.FrameSize ? 0 : (totalSize - Extractor.FrameSize) / Extractor.HopSize + 1;

            // if there's not enough data for copmuting even one feature vector
            // just copy data to temp buffer:

            if (vectorCount == 0)
            {
                data.FastCopyTo(_tempBuffer, data.Length, 0, _skippedCount);

                _skippedCount += data.Length;

                return new List<float[]>();
            }

            // otherwise pre-allocate memory and start processing

            var featureVectors = new List<float[]>(vectorCount);

            for (var i = 0; i < vectorCount; i++)
            {
                featureVectors.Add(new float[Extractor.FeatureCount]);
            }

            ComputeFrom(data, featureVectors);

            return featureVectors;
        }

        /// <summary>
        /// <para>Computes feature vectors from <paramref name="data"/> and stores them in <paramref name="featureVectors"/>.</para>
        /// <para>Returns the number of actually computed feature vectors.</para>
        /// </summary>
        /// <param name="data">Block of data</param>
        /// <param name="startSample">Index of the first sample in array for processing</param>
        /// <param name="endSample">Index of the last sample in array for processing</param>
        /// <param name="featureVectors">Pre-allocated sequence for storing the resulting feature vectors</param>
        public int ComputeFrom(float[] data, int startSample, int endSample, IList<float[]> featureVectors)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");
            return ComputeFrom(data.FastCopyFragment(endSample - startSample + 1, startSample), featureVectors);
        }

        /// <summary>
        /// <para>Computes feature vectors from <paramref name="data"/>.</para>
        /// <para>Returns the list of computed feature vectors or empty list, if the number of samples is less than the size of analysis frame.</para>
        /// </summary>
        /// <param name="data">Block of data</param>
        /// <param name="startSample">Index of the first sample in array for processing</param>
        /// <param name="endSample">Index of the last sample in array for processing</param>
        public List<float[]> ComputeFrom(float[] data, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");
            return ComputeFrom(data.FastCopyFragment(endSample - startSample + 1, startSample));
        }

        /// <summary>
        /// Resets online feature extractor.
        /// </summary>
        public void Reset()
        {
            Array.Clear(_tempBuffer, 0, _tempBuffer.Length);
            _skippedCount = 0;
        }
    }
}
