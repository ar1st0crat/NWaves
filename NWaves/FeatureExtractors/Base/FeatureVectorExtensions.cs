using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Provides extension methods for feature vectors.
    /// </summary>
    public static class FeatureVectorExtensions
    {
        /// <summary>
        /// Creates dictionary with feature vector statistics (keys are: min, max, mean, var).
        /// </summary>
        /// <param name="vector">Feature vector</param>
        public static Dictionary<string, float> Statistics(this float[] vector)
        {
            var mean = vector.Average();

            return new Dictionary<string, float>
            {
                { "min",  vector.Min() },
                { "max",  vector.Max() },
                { "mean", mean },
                { "var",  vector.Average(v => (v - mean) * (v - mean)) }
            };
        }
    }
}
