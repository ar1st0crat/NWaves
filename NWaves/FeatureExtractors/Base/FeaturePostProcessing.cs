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
                var f = new double[3 * featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    f[j] = vectors[i - N].Features[j];
                }
                for (var j = 0; j < featureCount; j++)
                {
                    var num = 0.0;
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
                    var num = 0.0;
                    for (var n = 1; n <= N; n++)
                    {
                        num += sequence[i + n].Features[j + featureCount] * n;
                        num -= sequence[i - n].Features[j + featureCount] * n;
                    }
                    vectors[i - N].Features[j + 2 * featureCount] = num / 10;
                }
            }
        }
    }
}
