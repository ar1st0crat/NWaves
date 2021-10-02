using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Utils;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Provides methods for post-processing of feature vector sequences.
    /// </summary>
    public static class FeaturePostProcessing
    {
        /// <summary>
        /// Does mean subtraction (in particular, CMN).
        /// </summary>
        /// <param name="vectors">Sequence of feature vectors</param>
        public static void NormalizeMean(IList<float[]> vectors)
        {
            if (vectors.Count < 2)
            {
                return;
            }

            var featureCount = vectors[0].Length;

            for (var i = 0; i < featureCount; i++)
            {
                var mean = vectors.Average(t => t[i]);
                
                foreach (var vector in vectors)
                {
                    vector[i] -= mean;
                }
            }
        }

        /// <summary>
        /// Does variance normalization (division by estimate of std.deviation (biased or not biased)).
        /// </summary>
        /// <param name="vectors">Sequence of feature vectors</param>
        /// <param name="bias">Bias in estimate of variance (1 = not biased, 0 = biased)</param>
        public static void NormalizeVariance(IList<float[]> vectors, int bias = 1)
        {
            var n = vectors.Count;

            if (n < 2)
            {
                return;
            }

            var featureCount = vectors[0].Length;

            for (var i = 0; i < featureCount; i++)
            {
                var mean = vectors.Average(t => t[i]);
                var std = vectors.Sum(t => (t[i] - mean) * (t[i] - mean) / (n - bias));

                if (std < Math.Abs(1e-30f))      // avoid dividing by zero
                {
                    std = 1;
                }

                foreach (var vector in vectors)
                {
                    vector[i] /= (float)Math.Sqrt(std);
                }
            }
        }

        /// <summary>
        /// Extends feature vectors with delta-features (1st and optionally 2nd order derivatives).
        /// 
        /// <para>According to formula:</para>
        /// 
        ///   <para>d_t = \frac{\sum_{n=1}^N n (c_{t+n} - c_{t-n})}{2 \sum_{n=1}^N n^2}</para>
        ///   
        /// <paramref name="N"/> vectors should be introduced before and after input vectors for computations.<br/>
        /// By default, these vectors will be created automatically and copied from the first and the last input vector.<br/>
        /// They can also be specified explicitly in <paramref name="previous"/> and <paramref name="next"/> parameters.
        /// </summary>
        /// <param name="vectors">Sequence of feature vectors that will be extended</param>
        /// <param name="previous">Sequence of <paramref name="N"/> feature vectors that will be prepended to <paramref name="vectors"/> for computations</param>
        /// <param name="next">Sequence of <paramref name="N"/> feature vectors that will be appended to <paramref name="vectors"/> for computations</param>
        /// <param name="includeDeltaDelta">Should delta-delta features be computed</param>
        /// <param name="N">Number of feature vectors before and after input <paramref name="vectors"/> for computations</param>
        public static void AddDeltas(IList<float[]> vectors, 
                                     IList<float[]> previous = null,
                                     IList<float[]> next = null,
                                     bool includeDeltaDelta = true,
                                     int N = 2)
        {
            if (previous is null)
            {
                previous = new List<float[]>(N);
                for (var n = 0; n < N; n++) previous.Add(vectors[0]);
            }
            if (next is null)
            {
                next = new List<float[]>(N);
                for (var n = 0; n < N; n++) next.Add(vectors.Last());
            }

            var featureCount = vectors[0].Length;

            var sequence = previous.Concat(vectors).Concat(next).ToArray();

            // deltas:

            int M = 2 * Enumerable.Range(1, N).Sum(x => x * x);  // scaling in denominator

            var newSize = includeDeltaDelta ? 3 * featureCount : 2 * featureCount;

            for (var i = N; i < sequence.Length - N; i++)
            {
                var f = new float[newSize];

                for (var j = 0; j < featureCount; j++)
                {
                    f[j] = vectors[i - N][j];
                }
                for (var j = 0; j < featureCount; j++)
                {
                    var num = 0.0f;
                    for (var n = 1; n <= N; n++)
                    {
                        num += n * (sequence[i + n][j] - sequence[i - n][j]);
                    }
                    f[j + featureCount] = num / M;
                }
                sequence[i] = vectors[i - N] = f;
            }

            if (!includeDeltaDelta) return;

            // delta-deltas:

            for (var n = 1; n <= N; n++)
            {
                sequence[n - 1] = vectors[0];
                sequence[sequence.Length - n] = vectors.Last();
            }

            for (var i = N; i < sequence.Length - N; i++)
            {
                for (var j = 0; j < featureCount; j++)
                {
                    var num = 0.0f;
                    for (var n = 1; n <= N; n++)
                    {
                        num += n * (sequence[i + n][j + featureCount] -
                                    sequence[i - n][j + featureCount]);
                    }
                    vectors[i - N][j + 2 * featureCount] = num / M;
                }
            }
        }

        /// <summary>
        /// Joins (merges) feature vectors from different collections into one combined feature vector. 
        /// For example, it can join 12 MFCC and 10 PLP coeffs into one 22-dimensional vector. 
        /// Collections of feature vectors must have the same size and contain at least one vector.
        /// </summary>
        /// <param name="vectors">Sequences of feature vectors</param>
        public static float[][] Join(params IList<float[]>[] vectors)
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

            var length = vectors.Sum(v => v[0].Length);
            var joined = new float[totalVectors][];
            
            for (var i = 0; i < joined.Length; i++)
            {
                var features = new float[length];

                for (int offset = 0, j = 0; j < vectorCount; j++)
                {
                    var size = vectors[j][i].Length;
                    vectors[j][i].FastCopyTo(features, size, 0, offset);
                    offset += size;
                }

                joined[i] = features;
            }

            return joined;
        }
    }
}
