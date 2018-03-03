using System;

namespace NWaves.Features
{
    /// <summary>
    /// Spectral features
    /// </summary>
    public static class Spectral
    {
        /// <summary>
        /// Spectral centroid
        /// </summary>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <returns>Spectral centroid</returns>
        public static double Centroid(double[] spectrum)
        {
            var sum = 0.0;
            var weightedSum = 0.0;

            for (var i = 0; i < spectrum.Length; i++)
            {
                sum += spectrum[i];
                weightedSum += i * spectrum[i];
            }

            return weightedSum / sum;
        }

        /// <summary>
        /// Spectral flatness
        /// </summary>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <param name="minLevel"></param>
        /// <returns></returns>
        public static double Flatness(double[] spectrum, double minLevel = 1e-10)
        {
            var sumVal = 0.0;
            var logSumVal = 0.0;

            for (var i = 0; i < spectrum.Length; i++)
            {
                var v = Math.Max(spectrum[i], minLevel);

                sumVal += v;
                logSumVal += Math.Log(v);
            }

            sumVal = sumVal / spectrum.Length;
            logSumVal = logSumVal / spectrum.Length;

            return sumVal > 0 ? Math.Exp(logSumVal) / sumVal : 0.0;
        }

        /// <summary>
        /// Spectral rolloff
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="rollofPercent"></param>
        /// <returns></returns>
        public static double Rolloff(double[] spectrum, double rollofPercent = 0.85)
        {
            return 0.0;
        }

        public static double Bandwidth(double[] spectrum)
        {
            return 0.0;
        }

        public static double Contrast(double[] spectrum)
        {
            return 0.0;
        }
    }
}
