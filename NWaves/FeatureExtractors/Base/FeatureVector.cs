using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors.Base
{
    /// <summary>
    /// Feature vector
    /// </summary>
    public class FeatureVector
    {
        /// <summary>
        /// Array of feature values
        /// </summary>
        public float[] Features { get; set; }

        /// <summary>
        /// Position of the feature vector in time (in sec)
        /// </summary>
        public double TimePosition { get; set; }

        /// <summary>
        /// TODO: this dictionary is probably going to be put in a standalone class for statistics
        /// </summary>
        public Dictionary<string, float> Statistics => new Dictionary<string, float>
        {
            { "mean", Features.Average() },
            { "min", Features.Min() },
            { "max", Features.Max() }
        };
    }
}
