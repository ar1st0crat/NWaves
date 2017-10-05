using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// 
    /// </summary>
    public class FeatureVector
    {
        /// <summary>
        /// 
        /// </summary>
        public double[] Features { get; set; }

        /// <summary>
        /// TODO: this Dictionary is going to be a standalone class for statistics
        /// </summary>
        public Dictionary<string, double> Statistics => new Dictionary<string, double>
        {
            { "mean", Features.Average() },
            { "min", Features.Min() },
            { "max", Features.Max() }
        };
    }
}
