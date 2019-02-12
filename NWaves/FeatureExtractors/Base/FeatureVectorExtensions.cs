using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors.Base
{
    public static class FeatureVectorExtensions
    {
        public static IEnumerable<float[]> Featuregram(this IList<FeatureVector> vectors)
        {
            return vectors.Select(v => v.Features);
        }
    }
}
