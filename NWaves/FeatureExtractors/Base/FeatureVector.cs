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
    }
}
