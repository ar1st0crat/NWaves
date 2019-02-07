using System;

namespace NWaves.Features
{
    /// <summary>
    /// Harmonic features
    /// </summary>
    public static class Harmonic
    {
        /// <summary>
        /// Simple algorithm for detecting harmonic peaks in spectrum
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Array for peak positions</param>
        /// <param name="peakFrequencies">Array for peak frequencies</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="pitch">Pitch is given if it is known</param>
        public static void Peaks(float[] spectrum, int[] peaks, float[] peakFrequencies, int samplingRate, float pitch = -1)
        {
            if (pitch < 0)
            {
                pitch = Pitch.FromSpectralPeaks(spectrum, samplingRate);
            }

            var resolution = samplingRate / (2 * (spectrum.Length - 1));

            var region = (int)(pitch / (2 * resolution));

            peaks[0] = (int)(pitch / resolution);
            peakFrequencies[0] = pitch;

            for (var i = 0; i < peaks.Length; i++)
            {
                var candidate = (i + 1) * peaks[0];

                var c = candidate;
                for (var j = -region; j < region; j++)
                {
                    if (c + j - 1       > 0 &&
                        spectrum[c + j] > spectrum[c + j - 1] && 
                        spectrum[c + j] > spectrum[c + j + 1] &&
                        spectrum[c + j] > spectrum[candidate])
                    {
                        candidate = c + j;
                    }
                }

                peaks[i] = candidate;
                peakFrequencies[i] = resolution * candidate;
            }
        }

        /// <summary>
        /// Harmonic centroid
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions</param>
        /// <param name="peakFrequencies">Peak frequencies</param>
        /// <returns>Harmonic centroid</returns>
        public static float Centroid(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

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
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions</param>
        /// <param name="peakFrequencies">Peak frequencies</param>
        /// <returns>Harmonic spread</returns>
        public static float Spread(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var centroid = Centroid(spectrum, peaks, peakFrequencies);

            var sum = 0.0f;
            var weightedSum = 0.0f;

            for (var i = 0; i < peaks.Length; i++)
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
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions</param>
        /// <param name="peakFrequencies">Peak frequencies</param>
        /// <returns>Inharmonicity</returns>
        public static float Inharmonicity(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var f0 = peakFrequencies[0];

            var squaredSum = 0.0f;
            var sum = 0.0f;

            for (var i = 0; i < peaks.Length; i++)
            {
                var p = peaks[i];
                var sqr = spectrum[p] * spectrum[p];

                sum += (peakFrequencies[i] - (i + 1) * f0) * sqr;
                squaredSum += sqr;
            }

            return 2 * sum / (f0 * squaredSum);
        }

        /// <summary>
        /// Harmonic Odd-to-Even Ratio
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions</param>
        /// <returns>Odd-to-Even Ratio</returns>
        public static float OddToEvenRatio(float[] spectrum, int[] peaks)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var oddSum = 0.0f;
            var evenSum = 0.0f;

            for (var i = 0; i < peaks.Length; i += 2)
            {
                evenSum += spectrum[peaks[i]];
            }

            for (var i = 1; i < peaks.Length; i += 2)
            {
                oddSum += spectrum[peaks[i]];
            }

            return oddSum / evenSum;
        }

        /// <summary>
        /// Tristimulus (nth component)
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions</param>
        /// <param name="n">Tristimulus component: 1, 2 or 3</param>
        /// <returns>Tristimulus</returns>
        public static float Tristimulus(float[] spectrum, int[] peaks, int n)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var sum = 0.0f;

            for (var i = 0; i < peaks.Length; i++)
            {
                sum += spectrum[peaks[i]];
            }

            if (n == 1)
            {
                return spectrum[peaks[0]] / sum;
            }
            else if (n == 2)
            {
                return (spectrum[peaks[1]] + spectrum[peaks[2]] + spectrum[peaks[3]]) / sum;
            }
            else
            {
                return (sum - spectrum[peaks[0]] - spectrum[peaks[1]] - spectrum[peaks[2]] - spectrum[peaks[3]]) / sum;
            }
        }
    }
}
