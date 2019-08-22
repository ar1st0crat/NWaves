using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace NWaves.FeatureExtractors.Options
{
    public static class FeatureExtractorOptionsExtensions
    {
        /// <summary>
        /// Simple JSON serialization
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
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
        /// Simple JSON deserialization
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
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
        /// Cast options of one type to options of another type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
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
