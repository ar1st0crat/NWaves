using System.Collections.Generic;
using System.Globalization;
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
        /// Delimiter
        /// </summary>
        private readonly char _delimiter;

        /// <summary>
        /// Constructor accepting the list of feature vectors
        /// </summary>
        /// <param name="featureVectors">List of feature vectors for serialization</param>
        /// <param name="featureNames">List of feature vectors for serialization</param>
        public CsvFeatureSerializer(IEnumerable<FeatureVector> featureVectors,
                                    IEnumerable<string> featureNames = null,
                                    char delimiter = ',')
        {
            _vectors = featureVectors;
            _names = featureNames;
            _delimiter = delimiter;
        }

        /// <summary>
        /// Asynchronous method for feature vectors serialization
        /// </summary>
        public async Task SerializeAsync(Stream stream)
        {
            var comma = _delimiter.ToString();

            using (var writer = new StreamWriter(stream))
            {
                if (_names != null)
                {
                    var header = $"time_pos{comma}{string.Join(comma, _names)}";
                    await writer.WriteLineAsync(header).ConfigureAwait(false);
                }

                foreach (var vector in _vectors)
                {
                    var line = string.Format("{0}{1}{2}",
                                         vector.TimePosition.ToString("0.000", CultureInfo.InvariantCulture),
                                         comma,
                                         string.Join(comma, vector.Features.Select(f => f.ToString("0.00000", CultureInfo.InvariantCulture))));

                    await writer.WriteLineAsync(line).ConfigureAwait(false);
                }
            }
        }
    }
}
