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
        /// Method for complementing feature vectors with derivatives.
        /// </summary>
        /// <param name="vectors"></param>
        public static void AddDeltas(List<FeatureVector> vectors)
        {
            throw new NotImplementedException();
        }
    }
}
