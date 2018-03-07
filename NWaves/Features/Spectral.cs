using System;
using System.Linq;

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
        /// <param name="frequencies">Centre frequencies</param>
        /// <returns>Spectral centroid</returns>
        public static double Centroid(double[] spectrum, double[] frequencies)
        {
            var sum = 0.0;
            var weightedSum = 0.0;

            for (var i = 1; i < spectrum.Length; i++)
            {
                sum += spectrum[i];
                weightedSum += frequencies[i] * spectrum[i];
            }

            return weightedSum / sum;
        }

        /// <summary>
        /// Spectral spread (variance)
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="frequencies"></param>
        /// <returns></returns>
        public static double Spread(double[] spectrum, double[] frequencies)
        {
            var mean = 0.0;
            for (var i = 1; i < spectrum.Length; i++)
            {
                mean += spectrum[i];
            }
            mean /= spectrum.Length;

            var sum = 0.0;
            var weightedSum = 0.0;

            for (var i = 1; i < spectrum.Length; i++)
            {
                sum += spectrum[i];
                weightedSum += spectrum[i] * (frequencies[i] - mean) * (frequencies[i] - mean);
            }

            return weightedSum / sum;
        }

        /// <summary>
        /// Spectral flatness
        /// </summary>
        /// <param name="spectrum">Magnitude spectrum</param>
        /// <param name="frequencies">Centre frequencies</param>
        /// <param name="minLevel"></param>
        /// <returns></returns>
        public static double Flatness(double[] spectrum, double[] frequencies, double minLevel = 1e-10)
        {
            var sum = 0.0;
            var logSum = 0.0;

            for (var i = 1; i < spectrum.Length; i++)
            {
                var amp = Math.Max(spectrum[i], minLevel);

                sum += amp;
                logSum += Math.Log(amp);
            }

            sum /= spectrum.Length;
            logSum /= spectrum.Length;

            return sum > 0 ? Math.Exp(logSum) / sum : 0.0;
        }

        /// <summary>
        /// Spectral rolloff frequency
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="frequencies">Centre frequencies</param>
        /// <param name="rolloffPercent"></param>
        /// <returns></returns>
        public static double Rolloff(double[] spectrum, double[] frequencies, double rolloffPercent = 0.85)
        {
            var threshold = 0.0;
            for (var i = 1; i < spectrum.Length; i++)
            {
                threshold += spectrum[i];
            }

            threshold *= rolloffPercent;
            
            var cumulativeSum = 0.0;
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
        /// Spectral bandwidth:
        /// 
        ///  (sum_k { S[k] * (freq[k] - centroid)^p }) ^ (1/p)
        /// 
        /// NB. S[k] is normalized (S[k] = s[k] / sum{|s[k]|})
        ///   
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="frequencies">Centre frequencies</param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static double Bandwidth(double[] spectrum, double[] frequencies, double p = 2)
        {
            var centroid = Centroid(spectrum, frequencies);

            var norm = spectrum.Sum(s => Math.Abs(s));

            var sum = 0.0;
            for (var i = 1; i < spectrum.Length; i++)
            {
                sum += spectrum[i] / norm * Math.Pow(Math.Abs(frequencies[i] - centroid), p);
            }

            return Math.Pow(sum, 1/p);
        }

        /// <summary>
        /// Spectral crest
        /// </summary>
        /// <param name="spectrum"></param>
        /// <returns></returns>
        public static double Crest(double[] spectrum)
        {
            var sum = 0.0;
            var max = 0.0;
            
            for (var i = 1; i < spectrum.Length; i++)
            {
                var s = spectrum[i] * spectrum[i];

                sum += s;

                if (s > max)
                {
                    max = s;
                }
            }

            return sum > 0 ? spectrum.Length * max / sum : 1.0;
        }

        /// <summary>
        /// Spectral contrast (array of *bandCount* values)
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="frequencies"></param>
        /// <param name="minFrequency"></param>
        /// <param name="bandCount"></param>
        /// <returns></returns>
        public static double[] Contrast(double[] spectrum, double[] frequencies, double minFrequency = 200.0, int bandCount = 6)
        {
            const double alpha = 0.02;

            var contrasts = new double[bandCount];

            var octaveLow = minFrequency;
            var octaveHigh = 2 * octaveLow;

            for (var n = 0; n < bandCount; n++)
            {
                var bandSpectrum = spectrum.Where((s, i) => frequencies[i] >= octaveLow && frequencies[i] <= octaveHigh)
                                           .OrderBy(s => s)
                                           .ToArray();

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

                contrasts[n] = Math.Log10(avgPeaks / avgValleys);

                octaveLow *= 2;
                octaveHigh *= 2;
            }

            return contrasts;
        }

        /// <summary>
        /// Spectral contrast in one particular spectral band (#bandNo).
        /// This function is called from SpectralFeatureExtractor.
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="frequencies"></param>
        /// <param name="bandNo"></param>
        /// <param name="minFrequency"></param>
        /// <returns></returns>
        public static double Contrast(double[] spectrum, double[] frequencies, int bandNo, double minFrequency = 200.0)
        {
            const double alpha = 0.02;

            var octaveLow = minFrequency * Math.Pow(2, bandNo - 1);
            var octaveHigh = 2 * octaveLow;

            var bandSpectrum = spectrum.Where((s,i) => frequencies[i] >= octaveLow && frequencies[i] <= octaveHigh)
                                       .OrderBy(s => s)
                                       .ToArray();

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

            return Math.Log10(avgPeaks / avgValleys);
        }
    }
}
