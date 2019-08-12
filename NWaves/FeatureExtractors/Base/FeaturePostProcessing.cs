using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Utils;

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
        /// <param name="vectors">Sequence of feature vectors</param>
        public static void NormalizeMean(IList<FeatureVector> vectors)
        {
            if (vectors.Count < 2)
            {
                return;
            }

            var featureCount = vectors[0].Features.Length;

            for (var i = 0; i < featureCount; i++)
            {
                var mean = vectors.Average(t => t.Features[i]);
                
                foreach (var vector in vectors)
                {
                    vector.Features[i] -= mean;
                }
            }
        }

        /// <summary>
        /// Variance normalization (divide by unbiased estimate of stdev)
        /// </summary>
        /// <param name="vectors">Sequence of feature vectors</param>
        public static void NormalizeVariance(IList<FeatureVector> vectors, int bias = 1)
        {
            var n = vectors.Count;

            if (n < 2)
            {
                return;
            }

            var featureCount = vectors[0].Features.Length;

            for (var i = 0; i < featureCount; i++)
            {
                var mean = vectors.Average(t => t.Features[i]);
                var std = vectors.Sum(t => (t.Features[i] - mean) * (t.Features[i] - mean) / (n - bias));

                if (std < Math.Abs(1e-30f))      // avoid dividing by zero
                {
                    std = 1;
                }

                foreach (var vector in vectors)
                {
                    vector.Features[i] /= (float)Math.Sqrt(std);
                }
            }
        }

        /// <summary>
        /// Method for complementing feature vectors with 1st and (by default) 2nd order derivatives.
        /// </summary>
        /// <param name="vectors"></param>
        /// <param name="previous"></param>
        /// <param name="next"></param>
        /// <param name="includeDeltaDelta"></param>
        /// <param name="N"></param>
        public static void AddDeltas(IList<FeatureVector> vectors, 
                                     IList<FeatureVector> previous = null,
                                     IList<FeatureVector> next = null,
                                     bool includeDeltaDelta = true,
                                     int N = 2)
        {
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

            int M = 2 * Enumerable.Range(1, N).Sum(x => x * x);  // scaling in denominator

            for (var i = N; i < sequence.Length - N; i++)
            {
                var f = includeDeltaDelta ? new float[3 * featureCount] : new float[2 * featureCount];

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
                    f[j + featureCount] = num / M;
                }
                vectors[i - N].Features = f;
            }

            if (!includeDeltaDelta) return;

            // delta-deltas:

            vectors[0].Features.FastCopyTo(sequence[0].Features, featureCount, featureCount, featureCount);
            vectors[0].Features.FastCopyTo(sequence[1].Features, featureCount, featureCount, featureCount);
            vectors.Last().Features.FastCopyTo(sequence[vectors.Count].Features, featureCount, featureCount, featureCount);
            vectors.Last().Features.FastCopyTo(sequence[vectors.Count+1].Features, featureCount, featureCount, featureCount);

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
                    vectors[i - N].Features[j + 2 * featureCount] = num / M;
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

            switch (vectorCount)
            {
                case 0:
                    throw new ArgumentException("Empty collection of feature vectors!");
                case 1:
                    return vectors.ElementAt(0).ToArray();
            }

            var totalVectors = vectors[0].Count;
            if (vectors.Any(v => v.Count != totalVectors))
            {
                throw new InvalidOperationException("All sequences of feature vectors must have the same length!");
            }

            var length = vectors.Sum(v => v[0].Features.Length);
            var joined = new FeatureVector[totalVectors];
            
            for (var i = 0; i < joined.Length; i++)
            {
                var features = new float[length];

                for (int offset = 0, j = 0; j < vectorCount; j++)
                {
                    var size = vectors[j][i].Features.Length;
                    vectors[j][i].Features.FastCopyTo(features, size, 0, offset);
                    offset += size;
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
