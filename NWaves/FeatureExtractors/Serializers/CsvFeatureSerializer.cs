using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NWaves.FeatureExtractors.Base;

namespace NWaves.FeatureExtractors.Serializers
{
    /// <summary>
    /// Class for simple CSV serialization of feature vectors
    /// </summary>
    public class CsvFeatureSerializer
    {
        /// <summary>
        /// List of feature vectors for serialization
        /// </summary>
        private readonly List<FeatureVector> _vectors;

        /// <summary>
        /// Constructor accepting the list of feature vectors
        /// </summary>
        /// <param name="featureVectors">List of feature vectors for serialization</param>
        public CsvFeatureSerializer(List<FeatureVector> featureVectors)
        {
            _vectors = featureVectors;
        }

        /// <summary>
        /// Asynchronous method for feature vectors serialization
        /// </summary>
        public async Task SaveToAsync(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                foreach (var vector in _vectors)
                {
                    var line = vector.TimePosition.ToString("F3") + ";" +
                               string.Join(";", vector.Features.Select(f => f.ToString("F5"))) + ";";

                    await writer.WriteLineAsync(line).ConfigureAwait(false);
                }
            }
        }
    }
}
