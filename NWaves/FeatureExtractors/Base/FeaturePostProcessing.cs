using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Class providing methods for additional processing of feature vector sequences.
    /// </summary>
    public static class FeaturePostProcessing
    {
        /// <summary>
        /// Method for mean subtraction (in particular, CMN).
        /// </summary>
        /// <param name="vectors"></param>
        public static void NormalizeMean(IList<FeatureVector> vectors)
        {
            var featureCount = vectors[0].Features.Length;

            for (var i = 0; i < featureCount; i++)
            {
                var cmean = vectors.Average(t => t.Features[i]);

                for (var j = 0; j < vectors.Count; j++)
                {
                    vectors[j].Features[i] -= cmean;
                }
            }
        }

        /// <summary>
        /// Method for complementing feature vectors with 1st and 2nd order derivatives.
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        public static void AddDeltas(IList<FeatureVector> vectors, 
                                     IList<FeatureVector> previous = null,
                                     IList<FeatureVector> next = null)
        {
            const int N = 2;

            if (previous == null)
            {
                previous = new List<FeatureVector> { vectors[0], vectors[0] };
            }
            if (next == null)
            {
                next = new List<FeatureVector> { vectors.Last(), vectors.Last() };
            }

            var featureCount = vectors[0].Features.Length;

            var sequence = previous.Concat(vectors).Concat(next).ToArray();
            
            // deltas:

            for (var i = N; i < sequence.Length - N; i++)
            {
                var f = new float[3 * featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    f[j] = vectors[i - N].Features[j];
                }
                for (var j = 0; j < featureCount; j++)
                {
                    var num = 0.0f;
                    for (var n = 1; n <= N; n++)
                    {
                        num += n * (sequence[i + n].Features[j] - sequence[i - n].Features[j]);
                    }
                    f[j + featureCount] = num / 10;
                }
                vectors[i - N].Features = f;
            }

            // delta-deltas:

            for (var i = N; i < sequence.Length - N; i++)
            {
                for (var j = 0; j < featureCount; j++)
                {
                    var num = 0.0f;
                    for (var n = 1; n <= N; n++)
                    {
                        num += n * (sequence[i + n].Features[j + featureCount] -
                                    sequence[i - n].Features[j + featureCount]);
                    }
                    vectors[i - N].Features[j + 2 * featureCount] = num / 10;
                }
            }
        }

        /// <summary>
        /// Join different collections of feature vectors.
        /// Time positions must coincide.
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static FeatureVector[] Join(params IList<FeatureVector>[] vectors)
        {
            var vectorCount = vectors.Length;

            if (vectorCount == 0)
            {
                throw new ArgumentException("Empty collection of feature vectors!");
            }

            if (vectorCount == 1)
            {
                return vectors.ElementAt(0).ToArray();
            }

            var totalVectors = vectors[0].Count;
            if (vectors.Any(v => v.Count != totalVectors))
            {
                throw new InvalidOperationException("");
            }

            var length = vectors.Sum(v => v[0].Features.Length);
            var joined = new FeatureVector[vectors[0].Count];
            
            for (var i = 0; i < joined.Length; i++)
            {
                var features = new float[length];

                var offset = 0;
                for (var j = 0; j < vectorCount; j++)
                {
                    Buffer.BlockCopy(vectors[j][i].Features, 0, features, offset, vectors[j][i].Features.Length * 4);
                    offset += vectors[j][i].Features.Length * 4;
                }

                joined[i] = new FeatureVector
                {
                    TimePosition = vectors[0][i].TimePosition,
                    Features = features
                };
            }

            return joined;
        }
    }
}
