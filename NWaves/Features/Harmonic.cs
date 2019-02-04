using System;

namespace NWaves.Features
{
    /// <summary>
    /// Harmonic features
    /// </summary>
    public static class Harmonic
    {
        /// <summary>
        /// Simple algorithm for detecting harmonic peaks
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="peaks"></param>
        /// <param name="peakFrequencies"></param>
        /// <param name="samplingRate"></param>
        /// <param name="pitch">Pitch is given if it is known</param>
        public static void Peaks(float[] spectrum, int[] peaks, float[] peakFrequencies, int samplingRate, float pitch = -1)
        {
            if (pitch < 0)
            {
                pitch = Pitch.FromHss(spectrum, samplingRate);
            }

            var resolution = samplingRate / (2 * (spectrum.Length - 1));

            peaks[0] = (int)(pitch / resolution) + 1;
            peakFrequencies[0] = pitch;

            for (var i = 1; i < peaks.Length; i++)
            {
                var candidate = (i + 1) * peaks[0];

                // TODO:
                // improve peak choosing

                peaks[i] = candidate;
                peakFrequencies[i] = resolution * candidate;
            }
        }

        /// <summary>
        /// Harmonic centroid
        /// </summary>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <param name="peakFrequencies"></param>
        /// <returns>Spectral centroid</returns>
        public static float Centroid(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            var sum = 0.0f;
            var weightedSum = 0.0f;

            for (var i = 0; i < peaks.Length; i++)
            {
                var p = peaks[i];
                sum += spectrum[p];
                weightedSum += peakFrequencies[i] * spectrum[p];
            }

            return weightedSum / sum;
        }

        /// <summary>
        /// Harmonic spread
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="spectrum"></param>
        /// <returns></returns>
        public static float Spread(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            var centroid = Centroid(spectrum, peaks, peakFrequencies);

            var sum = 0.0f;
            var weightedSum = 0.0f;

            for (var i = 1; i < spectrum.Length; i++)
            {
                var p = peaks[i];
                sum += spectrum[p];
                weightedSum += spectrum[p] * (peakFrequencies[i] - centroid) * (peakFrequencies[i] - centroid);
            }

            return (float)Math.Sqrt(weightedSum / sum);
        }

        /// <summary>
        /// Inharmonicity
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <returns>Spectral centroid</returns>
        public static float Inharmonicity(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            var f0 = peakFrequencies[0];

            var squaredSum = 0.0f;
            var sum = 0.0f;

            for (var i = 0; i < peaks.Length; i++)
            {
                var p = peaks[i];
                var sqr = spectrum[p] * spectrum[p];

                sum += (peakFrequencies[i] - i * f0) * sqr;
                squaredSum += sqr;
            }

            return 2 * sum / (f0 * squaredSum);
        }

        /// <summary>
        /// Harmonic Odd-to-Even Ratio
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <returns>Spectral centroid</returns>
        public static float OddToEvenRatio(float[] spectrum, int[] peaks)
        {
            var oddSum = 0.0f;
            var evenSum = 0.0f;

            for (var i = 1; i < peaks.Length; i += 2)
            {
                oddSum += spectrum[peaks[i]];
            }

            for (var i = 0; i < peaks.Length; i+=2)
            {
                evenSum += spectrum[peaks[i]];
            }

            return oddSum / evenSum;
        }

        /// <summary>
        /// Tristimulus (1st component)
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <returns>Spectral centroid</returns>
        public static float Tristimulus1(float[] spectrum, int[] peaks)
        {
            var sum = 0.0f;

            for (var i = 0; i < peaks.Length; i++)
            {
                sum += spectrum[peaks[i]];
            }

            return spectrum[peaks[0]] / sum;
        }

        /// <summary>
        /// Tristimulus (2nd component)
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <returns>Spectral centroid</returns>
        public static float Tristimulus2(float[] spectrum, int[] peaks)
        {
            var sum = 0.0f;

            for (var i = 0; i < peaks.Length; i++)
            {
                sum += spectrum[peaks[i]];
            }

            return (spectrum[peaks[1]] + spectrum[peaks[2]] + spectrum[peaks[3]]) / sum;
        }

        /// <summary>
        /// Tristimulus (3rd component)
        /// </summary>
        /// <param name="pitch"></param>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <returns>Spectral centroid</returns>
        public static float Tristimulus3(float[] spectrum, int[] peaks)
        {
            var sum = 0.0f;

            for (var i = 4; i < peaks.Length; i++)
            {
                sum += spectrum[peaks[i]];
            }

            return sum / (sum + spectrum[peaks[0]] + spectrum[peaks[1]] + spectrum[peaks[2]] + spectrum[peaks[3]]);
        }
    }
}
