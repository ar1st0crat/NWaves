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
        public static void NormalizeMean(List<FeatureVector> vectors)
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
        public static void AddDeltas(List<FeatureVector> vectors)
        {
            var featureCount = vectors[0].Features.Length;

            for (var i = 0; i < vectors.Count; i++)
            {
                var f = new double[2 * featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    f[j] = vectors[i].Features[j];
                }
                for (var j = 0; j < featureCount; j++)
                {
                    for (var n = 0; n < 2; n++)
                    {
                        if (i + 1 < vectors.Count)
                        {
                            f[j + featureCount] += vectors[i + 1].Features[j];

                            if (i + 2 < vectors.Count)
                            {
                                f[j + featureCount] += 2 * vectors[i + 2].Features[j];
                            }
                            else
                            {
                                f[j + featureCount] += 2 * vectors[i + 1].Features[j];
                            }
                        }
                        else
                        {
                            f[j + featureCount] += 3 * vectors[i].Features[j];
                        }

                        if (i > 1)
                        {
                            f[j + featureCount] -= vectors[i - 1].Features[j];

                            if (i > 2)
                            {
                                f[j + featureCount] -= 2 * vectors[i - 2].Features[j];
                            }
                            else
                            {
                                f[j + featureCount] -= 2 * vectors[i - 1].Features[j];
                            }
                        }
                        else
                        {
                            f[j + featureCount] -= 3 * vectors[i].Features[j];
                        }
                    }

                    f[j + featureCount] /= 10;
                }

                vectors[i].Features = f;
            }
        }
    }
}
