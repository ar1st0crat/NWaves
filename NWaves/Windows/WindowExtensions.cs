using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Windows
{
    /// <summary>
    /// Class providing extension methods for applying windows to signals and arrays of samples.
    /// </summary>
    public static class WindowExtensions
    {
        /// <summary>
        /// Apply window to array of <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">Signal samples</param>
        /// <param name="windowSamples">Window coefficients</param>
        public static void ApplyWindow(this float[] samples, float[] windowSamples)
        {
            for (var k = 0; k < windowSamples.Length; k++)
            {
                samples[k] *= windowSamples[k];
            }
        }

        /// <summary>
        /// Apply window to array of <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">Signal samples</param>
        /// <param name="windowSamples">Window coefficients</param>
        public static void ApplyWindow(this double[] samples, double[] windowSamples)
        {
            for (var k = 0; k < windowSamples.Length; k++)
            {
                samples[k] *= windowSamples[k];
            }
        }

        /// <summary>
        /// Apply window to <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="windowSamples">Window coefficients</param>
        public static void ApplyWindow(this DiscreteSignal signal, float[] windowSamples)
        {
            signal.Samples.ApplyWindow(windowSamples);
        }

        /// <summary>
        /// Apply window with optional <paramref name="parameters"/> to array of <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">Signal samples</param>
        /// <param name="window">Window type</param>
        /// <param name="parameters">Window parameters</param>
        public static void ApplyWindow(this float[] samples, WindowType window, params object[] parameters)
        {
            var windowSamples = Window.OfType(window, samples.Length, parameters);
            samples.ApplyWindow(windowSamples);
        }

        /// <summary>
        /// Apply window with optional <paramref name="parameters"/> to array of <paramref name="samples"/>.
        /// </summary>
        /// <param name="samples">Signal samples</param>
        /// <param name="window">Window type</param>
        /// <param name="parameters">Window parameters</param>
        public static void ApplyWindow(this double[] samples, WindowType window, params object[] parameters)
        {
            var windowSamples = Window.OfType(window, samples.Length, parameters).ToDoubles();
            samples.ApplyWindow(windowSamples);
        }

        /// <summary>
        /// Apply window with optional <paramref name="parameters"/> to <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="window">Window type</param>
        /// <param name="parameters">Window parameters</param>
        public static void ApplyWindow(this DiscreteSignal signal, WindowType window, params object[] parameters)
        {
            var windowSamples = Window.OfType(window, signal.Length, parameters);
            signal.Samples.ApplyWindow(windowSamples);
        }
    }
}
