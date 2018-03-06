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
        /// 
        /// </summary>
        /// <param name="spectrum"></param>
        /// <returns></returns>
        public static double Contrast(double[] spectrum, int bandNo)
        {
            var minFrequency = 200.0;
            var bandCount = 6;

            var octaves = new double[bandCount + 2];

            octaves[0] = minFrequency;
            for (var i = 1; i <= bandCount + 1; i++)
            {
                octaves[i] = octaves[i - 1] * 2;
            }

            var valleys = new double[bandCount + 1, spectrum.Length];
            var peaks = new double[bandCount + 1, spectrum.Length];



            return 0.0;
        }
    }
}
