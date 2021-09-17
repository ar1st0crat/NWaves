using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NWaves.FeatureExtractors.Serializers
{
    /// <summary>
    /// Class for simple CSV serialization of feature vectors.
    /// </summary>
    public class CsvFeatureSerializer
    {
        /// <summary>
        /// Sequence of feature vectors for serialization.
        /// </summary>
        private readonly IList<float[]> _vectors;

        /// <summary>
        /// Sequence of time markers for serialization.
        /// </summary>
        private readonly IList<double> _timeMarkers;

        /// <summary>
        /// Sequence of feature names/annotations for serialization.
        /// </summary>
        private readonly IList<string> _names;

        /// <summary>
        /// Delimiter symbol.
        /// </summary>
        private readonly char _delimiter;

        /// <summary>
        /// Constructs <see cref="CsvFeatureSerializer"/> from the list of feature vectors for serialization.
        /// </summary>
        /// <param name="featureVectors">Sequence of feature vectors for serialization</param>
        /// <param name="timeMarkers">Sequence of time markers for serialization</param>
        /// <param name="featureNames">Sequence of feature names/annotations for serialization</param>
        /// <param name="delimiter">Delimiter char</param>
        public CsvFeatureSerializer(IList<float[]> featureVectors,
                                    IList<double> timeMarkers = null,
                                    IList<string> featureNames = null,
                                    char delimiter = ',')
        {
            _vectors = featureVectors;
            _timeMarkers = timeMarkers;
            _names = featureNames;
            _delimiter = delimiter;
        }

        /// <summary>
        /// Serialize feature vectors into <paramref name="stream"/> using <paramref name="format"/> for values.
        /// </summary>
        /// <param name="stream">Output stream</param>
        /// <param name="format">Format/precision of values</param>
        /// <param name="timeFormat">Format/precision of time markers</param>
        public async Task SerializeAsync(Stream stream, string format = "0.00000", string timeFormat = "0.000")
        {
            var comma = _delimiter.ToString();

            using (var writer = new StreamWriter(stream))
            {
                if (_names != null)
                {
                    var names = string.Join(comma, _names);
                    var header = _timeMarkers == null ? $"{names}" : $"time_pos{comma}{names}";
                    await writer.WriteLineAsync(header).ConfigureAwait(false);
                }

                if (_timeMarkers is null)
                {
                    foreach (var vector in _vectors)
                    {
                        var line = string.Join(comma, vector.Select(f => f.ToString(format, CultureInfo.InvariantCulture)));

                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }
                else
                {
                    for (var i = 0; i < _vectors.Count; i++)
                    {
                        var line = string.Format("{0}{1}{2}",
                                             _timeMarkers[i].ToString(timeFormat, CultureInfo.InvariantCulture),
                                             comma,
                                             string.Join(comma, _vectors[i].Select(f => f.ToString(format, CultureInfo.InvariantCulture))));

                        await writer.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
