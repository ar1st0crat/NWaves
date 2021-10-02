using System;
using System.Linq;

namespace NWaves.Features
{
    /// <summary>
    /// Provides methods for computing spectral features.
    /// </summary>
    public static class Spectral
    {
        /// <summary>
        /// Computes spectral centroid.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="frequencies">Center frequencies</param>
        public static float Centroid(float[] spectrum, float[] frequencies)
        {
            var sum = 1e-10f;
            var weightedSum = 0.0f;

            for (var i = 1; i < spectrum.Length; i++)
            {
                sum += spectrum[i];
                weightedSum += frequencies[i] * spectrum[i];
            }

            return weightedSum / sum;
        }

        /// <summary>
        /// Computes spectral spread.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="frequencies">Center frequencies</param>
        public static float Spread(float[] spectrum, float[] frequencies)
        {
            var centroid = Centroid(spectrum, frequencies);

            var sum = 1e-10f;
            var weightedSum = 0.0f;

            for (var i = 1; i < spectrum.Length; i++)
            {
                sum += spectrum[i];
                weightedSum += spectrum[i] * (frequencies[i] - centroid) * (frequencies[i] - centroid);
            }

            return (float) Math.Sqrt(weightedSum / sum);
        }

        /// <summary>
        /// Computes spectral decrease.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        public static float Decrease(float[] spectrum)
        {
            var sum = 1e-10f;
            var diffSum = 0.0f;

            for (var i = 2; i < spectrum.Length; i++)
            {
                sum += spectrum[i];
                diffSum += (spectrum[i] - spectrum[1]) / (i - 1);
            }

            return diffSum / sum;
        }

        /// <summary>
        /// Computes spectral flatness.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="minLevel">Amplitude threshold</param>
        public static float Flatness(float[] spectrum, float minLevel = 1e-10f)
        {
            var sum = 0.0f;
            var logSum = 0.0;

            for (var i = 1; i < spectrum.Length; i++)
            {
                var amp = Math.Max(spectrum[i], minLevel);

                sum += amp;
                logSum += Math.Log(amp);
            }

            sum /= spectrum.Length;
            logSum /= spectrum.Length;

            return sum > 1e-10 ? (float)Math.Exp(logSum) / sum : 0.0f;
        }

        /// <summary>
        /// Computes spectral noiseness.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="frequencies">Center frequencies</param>
        /// <param name="noiseFrequency">Lower frequency of noise</param>
        public static float Noiseness(float[] spectrum, float[] frequencies, float noiseFrequency = 3000/*Hz*/)
        {
            var noiseSum = 0.0f;
            var totalSum = 1e-10f;

            var i = 1;
            for (; i < spectrum.Length && frequencies[i] < noiseFrequency; i++)
            {
                totalSum += spectrum[i];
            }

            for (; i < spectrum.Length; i++)
            {
                noiseSum += spectrum[i];
                totalSum += spectrum[i];
            }

            return noiseSum / totalSum;
        }

        /// <summary>
        /// Computes spectral rolloff frequency.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="frequencies">Center frequencies</param>
        /// <param name="rolloffPercent">Rolloff percent</param>
        public static float Rolloff(float[] spectrum, float[] frequencies, float rolloffPercent = 0.85f)
        {
            var threshold = 0.0f;
            for (var i = 1; i < spectrum.Length; i++)
            {
                threshold += spectrum[i];
            }

            threshold *= rolloffPercent;
            
            var cumulativeSum = 0.0f;
            var index = 0;
            for (var i = 1; i < spectrum.Length; i++)
            {
                cumulativeSum += spectrum[i];

                if (cumulativeSum > threshold)
                {
                    index = i;
                    break;
                }
            }

            return frequencies[index];
        }

        /// <summary>
        /// Computes spectral crest.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        public static float Crest(float[] spectrum)
        {
            var sum = 0.0f;
            var max = 0.0f;
            
            for (var i = 1; i < spectrum.Length; i++)
            {
                var s = spectrum[i] * spectrum[i];

                sum += s;

                if (s > max)
                {
                    max = s;
                }
            }

            return sum > 1e-10 ? spectrum.Length * max / sum : 1.0f;
        }

        /// <summary>
        /// Computes array of spectral contrasts in spectral bands.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="frequencies">Center frequencies</param>
        /// <param name="minFrequency">Starting frequency</param>
        /// <param name="bandCount">Number of spectral bands</param>
        public static float[] Contrast(float[] spectrum, float[] frequencies, float minFrequency = 200/*Hz*/, int bandCount = 6)
        {
            const double alpha = 0.02;

            var contrasts = new float[bandCount];

            var octaveLow = minFrequency;
            var octaveHigh = 2 * octaveLow;

            for (var n = 0; n < bandCount; n++)
            {
                var bandSpectrum = spectrum.Where((s, i) => frequencies[i] >= octaveLow && frequencies[i] <= octaveHigh)
                                           .OrderBy(s => s)
                                           .ToArray();

                if (bandSpectrum.Length == 0)
                {
                    return contrasts;   // zeros
                }

                var selectedCount = Math.Max(alpha * bandSpectrum.Length, 1);

                var avgPeaks = 0.0;
                var avgValleys = 0.0;

                for (var i = 0; i < selectedCount; i++)
                {
                    avgValleys += bandSpectrum[i];
                    avgPeaks += bandSpectrum[bandSpectrum.Length - i - 1];
                }

                avgPeaks /= selectedCount;
                avgValleys /= selectedCount;

                contrasts[n] = (float)Math.Log10(avgPeaks / avgValleys);

                octaveLow *= 2;
                octaveHigh *= 2;
            }

            return contrasts;
        }

        /// <summary>
        /// Computes spectral contrast in a spectral band with index <paramref name="bandNo"/>. 
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="frequencies">Center frequencies</param>
        /// <param name="bandNo">Spectral band index</param>
        /// <param name="minFrequency">Starting frequency</param>
        public static float Contrast(float[] spectrum, float[] frequencies, int bandNo, float minFrequency = 200/*Hz*/)
        {
            const double alpha = 0.02;

            var octaveLow = minFrequency * Math.Pow(2, bandNo - 1);
            var octaveHigh = 2 * octaveLow;

            var bandSpectrum = spectrum.Where((s, i) => frequencies[i] >= octaveLow && frequencies[i] <= octaveHigh)
                                       .OrderBy(s => s)
                                       .ToArray();

            if (bandSpectrum.Length == 0)
            {
                return 0;
            }

            var selectedCount = Math.Max(alpha * bandSpectrum.Length, 1);

            var avgPeaks = 0.0;
            var avgValleys = 0.0;

            for (var i = 0; i < selectedCount; i++)
            {
                avgValleys += bandSpectrum[i];
                avgPeaks += bandSpectrum[bandSpectrum.Length - i - 1];
            }

            avgPeaks /= selectedCount;
            avgValleys /= selectedCount;

            return (float)Math.Log10(avgPeaks / avgValleys);
        }

        /// <summary>
        /// Computes Shannon entropy of a spectrum (spectrum is treated as p.d.f.)
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        public static float Entropy(float[] spectrum)
        {
            var entropy = 0.0;

            var sum = spectrum.Sum();

            if (sum < 1e-8)
            {
                return 0;
            }

            for (var i = 1; i < spectrum.Length; i++)
            {
                var p = spectrum[i] / sum;

                if (p > 1e-8)
                {
                    entropy += p * Math.Log(p, 2);
                }
            }

            return (float)(-entropy / Math.Log(spectrum.Length, 2));
        }
    }
}
