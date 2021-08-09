using NWaves.Utils;
using System;
using System.Collections.Generic;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// FeatureExtractor adapter for online processing
    /// </summary>
    public class OnlineFeatureExtractor
    {
        /// <summary>
        /// Underlying feature extractor (can be set & replaced anytime)
        /// </summary>
        public FeatureExtractor Extractor { get; set; }

        /// <summary>
        /// Should the last non-processed samples in the current block be ignored in the next block
        /// </summary>
        private readonly bool _ignoreLastSamples;

        /// <summary>
        /// The number of last non-processed samples in the current block that will be processed in the next block
        /// </summary>
        private int _skippedCount = 0;

        /// <summary>
        /// Temporary buffer for accumulated samples
        /// </summary>
        private float[] _tempBuffer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="extractor"></param>
        /// <param name="ignoreLastSamples"></param>
        /// <param name="maxDataSize"></param>
        public OnlineFeatureExtractor(FeatureExtractor extractor, bool ignoreLastSamples = false, int maxDataSize = 0)
        {
            Extractor = extractor;
            _ignoreLastSamples = ignoreLastSamples;
            
            _tempBuffer = maxDataSize > 0 ? (new float[maxDataSize]) : (new float[extractor.SamplingRate]);
        }

        /// <summary>
        /// Get the number of feature vectors that will be computed in the block of given size
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public int VectorCount(int dataSize)
        {
            return dataSize < Extractor.FrameSize ? 0 : (dataSize - Extractor.FrameSize) / Extractor.HopSize + 1;
        }

        /// <summary>
        /// Get the number of feature vectors that will be computed in the block of given duration
        /// </summary>
        /// <param name="seconds"></param>
        public int VectorCountFromSeconds(double seconds) => VectorCount((int)(Extractor.SamplingRate * seconds));

        /// <summary>
        /// Ensure internal buffer size
        /// </summary>
        /// <param name="dataSize"></param>
        public void EnsureSize(int dataSize)
        {
            if (_tempBuffer.Length < dataSize)
            {
                Array.Resize(ref _tempBuffer, dataSize);
            }
        }

        /// <summary>
        /// Ensure internal buffer size from seconds
        /// </summary>
        /// <param name="seconds"></param>
        public void EnsureSizeFromSeconds(double seconds)
        {
            var dataSize = (int)(seconds * Extractor.SamplingRate) + 1;

            if (_tempBuffer.Length < dataSize)
            {
                Array.Resize(ref _tempBuffer, dataSize);
            }
        }

        /// <summary>
        /// Process current block of data and fill the list of resulting feature vectors
        /// </summary>
        /// <param name="data"></param>
        /// <param name="featureVectors"></param>
        /// <returns>Number of feature vectors</returns>
        public int ComputeFrom(float[] data, List<float[]> featureVectors)
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
        /// Process current block of data and return new list of resulting feature vectors
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<float[]> ComputeFrom(float[] data)
        {
            var vectorCount = VectorCount(data.Length + _skippedCount);

            // if there's not enough data for copmuting even one feature vector
            // just copy data to temp buffer:

            if (vectorCount == 0)
            {
                data.FastCopyTo(_tempBuffer, data.Length, 0, _skippedCount);

                _skippedCount += data.Length;

                return null;
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
        /// Reset online feature extractor
        /// </summary>
        public void Reset()
        {
            Array.Clear(_tempBuffer, 0, _tempBuffer.Length);
            _skippedCount = 0;
        }
    }
}
