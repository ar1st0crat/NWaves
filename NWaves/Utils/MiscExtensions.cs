using System.Collections.Generic;
using System.Linq;

namespace NWaves.Utils
{
    public static class MiscExtensions
    {
        public static float[] ToFloat(this IEnumerable<double> values)
        {
            return values.Select(v => (float) v).ToArray();
        }

        public static double[] ToDouble(this IEnumerable<float> values)
        {
            return values.Select(v => (double) v).ToArray();
        }
    }
}