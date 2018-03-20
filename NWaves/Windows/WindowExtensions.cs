using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Windows
{
    /// <summary>
    /// A few helper functions for applying windows to signals and arrays of samples
    /// </summary>
    public static class WindowExtensions
    {
        /// <summary>
        /// Mutable function that applies window array to array of float samples
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="windowSamples"></param>
        public static void ApplyWindow(this float[] samples, float[] windowSamples)
        {
            for (var k = 0; k < windowSamples.Length; k++)
            {
                samples[k] *= windowSamples[k];
            }
        }

        /// <summary>
        /// Mutable function that applies window array to array of double samples
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="windowSamples"></param>
        public static void ApplyWindow(this double[] samples, double[] windowSamples)
        {
            for (var k = 0; k < windowSamples.Length; k++)
            {
                samples[k] *= windowSamples[k];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="windowSamples"></param>
        public static void ApplyWindow(this DiscreteSignal signal, float[] windowSamples)
        {
            signal.Samples.ApplyWindow(windowSamples);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="window"></param>
        public static void ApplyWindow(this float[] samples, WindowTypes window)
        {
            var windowSamples = Window.OfType(window, samples.Length);
            samples.ApplyWindow(windowSamples);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="window"></param>
        public static void ApplyWindow(this double[] samples, WindowTypes window)
        {
            var windowSamples = Window.OfType(window, samples.Length).ToDoubles();
            samples.ApplyWindow(windowSamples);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="window"></param>
        public static void ApplyWindow(this DiscreteSignal signal, WindowTypes window)
        {
            var windowSamples = Window.OfType(window, signal.Length);
            signal.Samples.ApplyWindow(windowSamples);
        }
    }
}
