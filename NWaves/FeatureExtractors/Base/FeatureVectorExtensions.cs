using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors.Base
{
    public static class FeatureVectorExtensions
    {
        /// <summary>
        /// Dictionary with statistics
        /// </summary>
        public static Dictionary<string, float> Statistics(this float[] vector)
        {
            var mean = vector.Average();

            return new Dictionary<string, float>
            {
                { "min",  vector.Min() },
                { "max",  vector.Max() },
                { "mean", mean },
                { "var",  vector.Average(v => (v - mean) * (v - mean)) },
            };
        }
    }
}
