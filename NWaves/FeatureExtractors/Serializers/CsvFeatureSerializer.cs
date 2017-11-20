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
        /// Collection of feature vectors for serialization
        /// </summary>
        private readonly IEnumerable<FeatureVector> _vectors;

        /// <summary>
        /// Collection of feature names for serialization
        /// </summary>
        private readonly IEnumerable<string> _names;

        /// <summary>
        /// Constructor accepting the list of feature vectors
        /// </summary>
        /// <param name="featureVectors">List of feature vectors for serialization</param>
        /// <param name="featureNames">List of feature vectors for serialization</param>
        public CsvFeatureSerializer(IEnumerable<FeatureVector> featureVectors, IEnumerable<string> featureNames = null)
        {
            _vectors = featureVectors;
            _names = featureNames;
        }

        /// <summary>
        /// Asynchronous method for feature vectors serialization
        /// </summary>
        public async Task SerializeAsync(Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            {
                if (_names != null)
                {
                    var header = "time_pos" + ";" + string.Join(";", _names) + ";";
                    await writer.WriteLineAsync(header).ConfigureAwait(false);
                }

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
