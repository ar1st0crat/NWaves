using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors.Base
{
    public static class FeatureVectorExtensions
    {
        /// <summary>
        /// Featuregram
        /// </summary>
        /// <param name="vectors"></param>
        /// <returns></returns>
        public static IEnumerable<float[]> Featuregram(this IList<FeatureVector> vectors)
        {
            return vectors.Select(v => v.Features);
        }

        /// <summary>
        /// Dictionary with statistics
        /// </summary>
        public static Dictionary<string, float> Statistics(this FeatureVector vector)
        {
            var mean = vector.Features.Average();

            return new Dictionary<string, float>
            {
                { "min",  vector.Features.Min() },
                { "max",  vector.Features.Max() },
                { "mean", mean },
                { "var",  vector.Features.Average(v => (v - mean) * (v - mean)) },
            };
        }
    }
}
