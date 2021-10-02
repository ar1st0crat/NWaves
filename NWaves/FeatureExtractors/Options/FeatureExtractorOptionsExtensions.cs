using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace NWaves.FeatureExtractors.Options
{
    /// <summary>
    /// Provides methods for serializing and casting feature extractor configuration options. 
    /// </summary>
    public static class FeatureExtractorOptionsExtensions
    {
        /// <summary>
        /// Serializes feature extractor configuration options to JSON.
        /// </summary>
        /// <param name="stream">Output stream for JSON data</param>
        /// <param name="options">Feature extractor configuration options</param>
        public static void SaveOptions(this Stream stream, FeatureExtractorOptions options)
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  "))
                {
                    var js = new DataContractJsonSerializer(options.GetType());
                    js.WriteObject(writer, options);
                    stream.Flush();
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        /// <summary>
        /// Deserializes feature extractor configuration options from JSON.
        /// </summary>
        /// <typeparam name="T">Options type</typeparam>
        /// <param name="stream">Input stream containing JSON data</param>
        public static T LoadOptions<T>(this Stream stream) where T : FeatureExtractorOptions
        {
            var currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                var js = new DataContractJsonSerializer(typeof(T));
                return (T)js.ReadObject(stream);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = currentCulture;
            }
        }

        /// <summary>
        /// Casts feature extractor configuration options of one type to options of another type.
        /// </summary>
        /// <typeparam name="T">Original options type</typeparam>
        /// <typeparam name="U">Required options type</typeparam>
        /// <param name="options">Feature extractor configuration options</param>
        public static U Cast<T, U>(this T options) where T : FeatureExtractorOptions
                                                   where U : FeatureExtractorOptions
        {
            byte[] data;

            using (var config = new MemoryStream())
            {
                config.SaveOptions(options);
                data = config.ToArray();
            }

            using (var config = new MemoryStream(data))
            {
                return config.LoadOptions<U>();
            }
        }
    }
}
