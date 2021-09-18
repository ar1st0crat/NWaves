using System;

namespace NWaves.Features
{
    /// <summary>
    /// Class providing methods for computing harmonic spectral features.
    /// </summary>
    public static class Harmonic
    {
        /// <summary>
        /// <para>Evaluate harmonic peaks (peak indices and frequencies) in spectrum.</para>
        /// <para>
        /// If <paramref name="pitch"/> is not specified explicitly, 
        /// it will be auto-estimated using method <see cref="Pitch.FromSpectralPeaks(float[], int, float, float)"/>.
        /// </para>
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Array for storing computed peak positions</param>
        /// <param name="peakFrequencies">Array for storing computed peak frequencies</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="pitch">Pitch in Hz, or any negative number if the pitch is unknown</param>
        public static void Peaks(float[] spectrum, int[] peaks, float[] peakFrequencies, int samplingRate, float pitch = -1)
        {
            if (pitch < 0)
            {
                pitch = Pitch.FromSpectralPeaks(spectrum, samplingRate);
            }

            var resolution = (float)samplingRate / (2 * (spectrum.Length - 1));

            var region = (int)(pitch / (2 * resolution));

            peaks[0] = (int)(pitch / resolution);
            peakFrequencies[0] = pitch;
            
            for (var i = 0; i < peaks.Length; i++)
            {
                var candidate = (i + 1) * peaks[0];

                if (candidate >= spectrum.Length)
                {
                    peaks[i] = spectrum.Length - 1;
                    peakFrequencies[i] = resolution * (spectrum.Length - 1);
                    continue;
                }

                var c = candidate;
                for (var j = -region; j < region; j++)
                {
                    if (c + j - 1       > 0 &&
                        c + j + 1       < spectrum.Length &&
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
        /// Compute harmonic centroid.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions (indices in spectrum)</param>
        /// <param name="peakFrequencies">Peak frequencies</param>
        public static float Centroid(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var sum = 1e-10f;
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
        /// Compute harmonic spread.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions (indices in spectrum)</param>
        /// <param name="peakFrequencies">Peak frequencies</param>
        public static float Spread(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var centroid = Centroid(spectrum, peaks, peakFrequencies);

            var sum = 1e-10f;
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
        /// Compute inharmonicity.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions (indices in spectrum)</param>
        /// <param name="peakFrequencies">Peak frequencies</param>
        public static float Inharmonicity(float[] spectrum, int[] peaks, float[] peakFrequencies)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var f0 = peakFrequencies[0];

            var squaredSum = 1e-10f;
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
        /// Compute harmonic odd-to-even ratio.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions (indices in spectrum)</param>
        public static float OddToEvenRatio(float[] spectrum, int[] peaks)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var oddSum = 1e-10f;
            var evenSum = 1e-10f;

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
        /// Compute tristimulus (<paramref name="n"/>th component).
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="peaks">Peak positions (indices in spectrum)</param>
        /// <param name="n">Tristimulus component index: 1, 2 or 3</param>
        public static float Tristimulus(float[] spectrum, int[] peaks, int n)
        {
            if (peaks[0] == 0)
            {
                return 0;
            }

            var sum = 1e-10f;

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
