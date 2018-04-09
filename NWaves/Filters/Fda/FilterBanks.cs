using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Static class with methods providing general shapes of filter banks:
    /// 
    ///     - triangular
    ///     - rectangular
    ///     - FIR bandpass (close to trapezoidal, slightly overlapping)
    ///     - BiQuad bandpass
    /// 
    /// ...and methods for obtaining the most widely used frequency bands:
    /// 
    ///     - Herz bands
    ///     - Mel bands
    ///     - Bark bands
    ///     - Critical bands
    ///     - ERB filterbank
    ///     - Octaves (from MPEG-7)
    /// 
    /// </summary>
    public static class FilterBanks
    {
        /// <summary>
        /// Method returns universal triangular filterbank based on given frequencies.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <returns>Array of triangular filters</returns>
        public static float[][] Triangular(int fftSize, int samplingRate, Tuple<double, double, double>[] frequencies)
        {
            var herzResolution = (double)samplingRate / fftSize;

            var left = frequencies.Select(f => (int)Math.Round(f.Item1 / herzResolution)).ToArray();
            var center = frequencies.Select(f => (int)Math.Round(f.Item2 / herzResolution)).ToArray();
            var right = frequencies.Select(f => (int)Math.Round(f.Item3 / herzResolution)).ToArray();

            var filterCount = frequencies.Length;
            var filterBank = new float[filterCount][];

            for (var i = 0; i < filterCount; i++)
            {
                filterBank[i] = new float[fftSize / 2 + 1];

                for (var j = left[i]; j < center[i]; j++)
                {
                    filterBank[i][j] = (float)(j - left[i]) / (center[i] - left[i]);
                }
                for (var j = center[i]; j < right[i]; j++)
                {
                    filterBank[i][j] = (float)(right[i] - j) / (right[i] - center[i]);
                }
            }

            return filterBank;
        }

        /// <summary>
        /// Method returns universal rectangular filterbank based on given frequencies.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <returns>Array of rectangular filters</returns>
        public static float[][] Rectangular(int fftSize, int samplingRate, Tuple<double, double, double>[] frequencies)
        {
            var herzResolution = (double)samplingRate / fftSize;

            var left = frequencies.Select(f => (int)Math.Round(f.Item1 / herzResolution)).ToArray();
            var right = frequencies.Select(f => (int)Math.Round(f.Item3 / herzResolution)).ToArray();

            var filterCount = frequencies.Length;
            var filterBank = new float[filterCount][];

            for (var i = 0; i < filterCount; i++)
            {
                filterBank[i] = new float[fftSize / 2 + 1];

                for (var j = left[i]; j < right[i]; j++)
                {
                    filterBank[i][j] = 1;
                }
            }

            return filterBank;
        }

        /// <summary>
        /// Method returns FIR bandpass (close to trapezoidal) filterbank based on given frequencies.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <returns>Array of trapezoidal FIR filters</returns>
        public static float[][] Trapezoidal(int fftSize, int samplingRate, Tuple<double, double, double>[] frequencies)
        {
            var filterBank = Rectangular(fftSize, samplingRate, frequencies);

            for (var i = 0; i < filterBank.Length; i++)
            {
                var filter = DesignFilter.Fir(fftSize / 4 + 1, filterBank[i].ToDoubles());
                filterBank[i] = filter.FrequencyResponse(fftSize).Magnitude.ToFloats();

                // normalize gain to 1.0

                var maxAmp = 0.0f;
                for (var j = 0; j < filterBank[i].Length; j++)
                {
                    if (filterBank[i][j] > maxAmp) maxAmp = filterBank[i][j];
                }
                for (var j = 0; j < filterBank[i].Length; j++)
                {
                    filterBank[i][j] /= maxAmp;
                }
            }

            return filterBank;
        }

        /// <summary>
        /// Method returns BiQuad bandpass overlapping filters based on given frequencies.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="frequencies">Array of frequency tuples (left, center, right) for each filter</param>
        /// <returns>Array of BiQuad bandpass filters</returns>
        public static float[][] BiQuad(int fftSize, int samplingRate, Tuple<double, double, double>[] frequencies)
        {
            var center = frequencies.Select(f => f.Item2).ToArray();

            var filterCount = frequencies.Length;
            var filterBank = new float[filterCount][];

            for (var i = 0; i < filterCount; i++)
            {
                var freq = center[i] / samplingRate;
                var filter = new BandPassFilter(freq, 2.0);

                filterBank[i] = filter.FrequencyResponse(fftSize).Magnitude.ToFloats();
            }

            return filterBank;
        }

        /// <summary>
        /// This general method returns frequency tuples for uniformly spaced frequency bands on any scale.
        /// </summary>
        /// <param name="scaleMapper">The function that converts Hz to other frequency scale</param>
        /// <param name="inverseMapper">The function that converts frequency from alternative scale back to Hz</param>
        /// <param name="filterCount">Number of filters</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        /// <returns>Array of frequency tuples for each filter</returns>
        private static Tuple<double, double, double>[] UniformBands(
                                                        Func<double, double> scaleMapper,
                                                        Func<double, double> inverseMapper,
                                                        int filterCount, 
                                                        int samplingRate, 
                                                        double lowFreq = 0, double highFreq = 0, 
                                                        bool overlap = true)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var startingFrequency = scaleMapper(lowFreq);

            var frequencyTuples = new Tuple<double, double, double>[filterCount];

            if (overlap)
            {
                var newResolution = (scaleMapper(highFreq) - scaleMapper(lowFreq)) / (filterCount + 1);

                var frequencies = Enumerable.Range(0, filterCount + 2)
                                            .Select(i => inverseMapper(startingFrequency + i * newResolution))
                                            .ToArray();
                
                for (var i = 0; i < filterCount; i++)
                {
                    frequencyTuples[i] = new Tuple<double, double, double>
                        (frequencies[i], frequencies[i + 1], frequencies[i + 2]);
                }
            }
            else
            {
                var newResolution = (scaleMapper(highFreq) - scaleMapper(lowFreq)) / filterCount;

                var frequencies = Enumerable.Range(0, filterCount + 1)
                                            .Select(i => inverseMapper(startingFrequency + i * newResolution))
                                            .ToArray();
                
                for (var i = 0; i < filterCount; i++)
                {
                    frequencyTuples[i] = new Tuple<double, double, double>
                        (frequencies[i], (frequencies[i] + frequencies[i + 1]) / 2, frequencies[i + 1]);
                }
            }

            return frequencyTuples;
        }

        /// <summary>
        /// Method returns frequency tuples for uniformly spaced frequency bands on Herz scale.
        /// </summary>
        /// <param name="combFilterCount">Number of filters</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        /// <returns>Array of frequency tuples for each Herz filter</returns>
        public static Tuple<double, double, double>[] HerzBands(
            int combFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = false)
        {
            // "x => x" means map frequency 1-to-1 (in Hz as it is)
            return UniformBands(x => x, x => x, combFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }

        /// <summary>
        /// Method returns frequency tuples for uniformly spaced frequency bands on Mel scale.
        /// </summary>
        /// <param name="melFilterCount">Number of mel filters to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        /// <returns>Array of frequency tuples for each Mel filter</returns>
        public static Tuple<double, double, double>[] MelBands(
            int melFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = true)
        {
            return UniformBands(Scale.HerzToMel, Scale.MelToHerz, melFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }

        /// <summary>
        /// Method returns frequency tuples for uniformly spaced frequency bands on Bark scale.
        /// </summary>
        /// <param name="barkFilterCount">Number of bark filters to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        /// <returns>Array of frequency tuples for each Bark filter</returns>
        public static Tuple<double, double, double>[] BarkBands(
            int barkFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = true)
        {
            return UniformBands(Scale.HerzToBark, Scale.BarkToHerz, barkFilterCount, samplingRate, lowFreq, highFreq, overlap);
        }
        
        /// <summary>
        /// Method returns frequency tuples for critical bands.
        /// </summary>
        /// <param name="filterCount">Number of filters to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Overlap parameter (is always false; added for consistency with other methods)</param>
        /// <returns>Array of frequency tuples for each Critical Band filter</returns>
        public static Tuple<double, double, double>[] CriticalBands(
            int filterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = false)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            double[] edgeFrequencies = { 20,   100,  200,  300,  400,  510,  630,  770,  920,  1080, 1270,  1480,  1720,
                                         2000, 2320, 2700, 3150, 3700, 4400, 5300, 6400, 7700, 9500, 12000, 15500, 20500 };

            double[] centerFrequencies = { 50,   150,  250,  350,  450,  570,  700,  840,  1000, 1170, 1370,  1600,
                                           1850, 2150, 2500, 2900, 3400, 4000, 4800, 5800, 7000, 8500, 10500, 13500, 17500 };

            var startIndex = 0;
            for (var i = 0; i < centerFrequencies.Length; i++)
            {
                if (centerFrequencies[i] < lowFreq) continue;
                startIndex = i;
                break;
            }

            var endIndex = 0;
            for (var i = centerFrequencies.Length - 1; i >= 0; i--)
            {
                if (centerFrequencies[i] > highFreq) continue;
                endIndex = i;
                break;
            }

            filterCount = Math.Min(endIndex - startIndex + 1, filterCount);

            var edges = edgeFrequencies.Skip(startIndex)
                                       .Take(filterCount + 1)
                                       .ToArray();

            var centers = centerFrequencies.Skip(startIndex)
                                           .Take(filterCount)
                                           .ToArray();

            var frequencyTuples = new Tuple<double, double, double>[filterCount];

            for (var i = 0; i < filterCount; i++)
            {
                frequencyTuples[i] = new Tuple<double, double, double>
                    (edges[i], centers[i], edges[i + 1]);
            }

            return frequencyTuples;
        }

        /// <summary>
        /// Method returns frequency tuples for octave bands.
        /// </summary>
        /// <param name="octaveCount">Number of octave filters to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="overlap">Flag indicating that bands should overlap</param>
        /// <returns>Array of frequency tuples for each octave filter</returns>
        public static Tuple<double, double, double>[] OctaveBands(
            int octaveCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool overlap = false)
        {
            if (lowFreq < 1e-10)
            {
                lowFreq = 62.5;//Hz
            }

            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var f1 = lowFreq;
            var f2 = lowFreq * 2;

            var frequencyTuples = new List<Tuple<double, double, double>>();

            if (overlap)
            {
                var f3 = f2 * 2;

                for (var i = 0; i < octaveCount && f3 < highFreq; i++)
                {
                    frequencyTuples.Add(new Tuple<double, double, double>(f1, f2, f3));
                    f1 = f2;
                    f2 = f3;
                    f3 *= 2;
                }
            }
            else
            {
                for (var i = 0; i < octaveCount && f2 < highFreq; i++)
                {
                    frequencyTuples.Add(new Tuple<double, double, double>(f1, (f1 + f2) / 2, f2));
                    f1 *= 2;
                    f2 *= 2;
                }
            }

            return frequencyTuples.ToArray();
        }

        /// <summary>
        /// Method creates overlapping ERB filters (ported from Malcolm Slaney's MATLAB code).
        /// </summary>
        /// <param name="erbFilterCount">Number of ERB filters</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="normalizeGain">True if gain should be normalized; false if all filters should have same height 1.0</param>
        /// <returns>Array of ERB filters</returns>
        public static float[][] Erb(
            int erbFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool normalizeGain = true)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            const double earQ = 9.26449;
            const double minBw = 24.7;
            const double bw = earQ * minBw;
            const int order = 1;

            var t = 1.0 / samplingRate;
            
            var frequencies = new double[erbFilterCount];
            for (var i = 1; i <= erbFilterCount; i++)
            {
                frequencies[erbFilterCount - i] =
                    -bw + Math.Exp(i * (-Math.Log(highFreq + bw) + Math.Log(lowFreq + bw)) / erbFilterCount) * (highFreq + bw);
            }

            var ucirc = new Complex[fftSize / 2 + 1];
            for (var i = 0; i < ucirc.Length; i++)
            {
                ucirc[i] = Complex.Exp((2 * Complex.ImaginaryOne * i * Math.PI) / fftSize);
            }

            var rootPos = Math.Sqrt(3 + Math.Pow(2, 1.5));
            var rootNeg = Math.Sqrt(3 - Math.Pow(2, 1.5));

            var fft = new Fft(fftSize);

            var erbFilterBank = new float[erbFilterCount][];
            
            for (var i = 0; i < erbFilterCount; i++)
            {
                var cf = frequencies[i];
                var erb = Math.Pow(Math.Pow(cf / earQ, order) + Math.Pow(minBw, order), 1.0 / order);
                var b = 1.019 * 2 * Math.PI * erb;

                var theta = 2 * cf * Math.PI * t;
                var itheta = Complex.Exp(2 * Complex.ImaginaryOne * theta);

                var a0 = t;
                var a2 = 0.0;
                var b0 = 1.0;
                var b1 = -2 * Math.Cos(theta) / Math.Exp(b * t);
                var b2 = Math.Exp(-2 * b * t);

                var common = -t * Math.Exp(-b * t);

                var k1 = Math.Cos(theta) + rootPos * Math.Sin(theta);
                var k2 = Math.Cos(theta) - rootPos * Math.Sin(theta);
                var k3 = Math.Cos(theta) + rootNeg * Math.Sin(theta);
                var k4 = Math.Cos(theta) - rootNeg * Math.Sin(theta);

                var a11 = common * k1;
                var a12 = common * k2;
                var a13 = common * k3;
                var a14 = common * k4;

                var gainArg = Complex.Exp(Complex.ImaginaryOne * theta - b * t);

                var gain = Complex.Abs(
                                    (itheta - gainArg * k1) *
                                    (itheta - gainArg * k2) *
                                    (itheta - gainArg * k3) *
                                    (itheta - gainArg * k4) *
                                    Complex.Pow(t * Math.Exp(b * t) / (-1.0/Math.Exp(b*t) + 1 + itheta*(1 - Math.Exp(b*t))), 4.0));

                var filter1 = new IirFilter(new[] { a0, a11, a2 }, new[] { b0, b1, b2 });
                var filter2 = new IirFilter(new[] { a0, a12, a2 }, new[] { b0, b1, b2 });
                var filter3 = new IirFilter(new[] { a0, a13, a2 }, new[] { b0, b1, b2 });
                var filter4 = new IirFilter(new[] { a0, a14, a2 }, new[] { b0, b1, b2 });

                var ir = new double[fftSize];
                ir[0] = 1.0;

                // for doubles the following code will work ok
                // (however there's a crucial lost of precision in case of floats):

                //var filter = filter1 * filter2 * filter3 * filter4;
                //ir = filter.ApplyTo(ir);

                // this code is ok both for floats and for doubles:

                ir = filter1.ApplyTo(ir);
                ir = filter2.ApplyTo(ir);
                ir = filter3.ApplyTo(ir);
                ir = filter4.ApplyTo(ir);

                var kernel = new DiscreteSignal(1, ir.Select(s => (float)(s / gain)));
                
                erbFilterBank[i] = fft.PowerSpectrum(kernel, false).Samples;
            }

            // normalize gain (by default)

            if (!normalizeGain)
            {
                return erbFilterBank;
            }

            foreach (var filter in erbFilterBank)
            {
                var sum = 0.0;
                for (var j = 0; j < filter.Length; j++)
                {
                    sum += Math.Abs(filter[j] * filter[j]);
                }

                var weight = Math.Sqrt(sum * samplingRate / fftSize);

                for (var j = 0; j < filter.Length; j++)
                {
                    filter[j] = (float)(filter[j] / weight);
                }
            }

            return erbFilterBank;
        }
        
        /// <summary>
        /// Method applies filters to spectrum and fills resulting filtered spectrum.
        /// </summary>
        /// <param name="filterbank"></param>
        /// <param name="spectrum"></param>
        /// <param name="filtered"></param>
        public static void Apply(float[][] filterbank, float[] spectrum, float[] filtered)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                filtered[i] = 0.0f;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    filtered[i] += filterbank[i][j] * spectrum[j];
                }
            }
        }

        /// <summary>
        /// Method applies filters to sequence of spectra
        /// </summary>
        /// <param name="filterbank"></param>
        /// <param name="spectrogram"></param>
        public static float[][] Apply(float[][] filterbank, IList<float[]> spectrogram)
        {
            var filtered = new float[spectrogram.Count][];

            for (var k = 0; k < filtered.Length; k++)
            {
                filtered[k] = new float[filterbank.Length];
            }

            for (var i = 0; i < filterbank.Length; i++)
            {
                for (var k = 0; k < filtered.Length; k++)
                {
                    filtered[k][i] = 0.0f;

                    for (var j = 0; j < spectrogram[i].Length; j++)
                    {
                        filtered[k][i] += filterbank[i][j] * spectrogram[k][j];
                    }
                }
            }

            return filtered;
        }

        /// <summary>
        /// Method applies filters to spectrum and then does Log10() on resulting spectrum.
        /// </summary>
        /// <param name="filterbank"></param>
        /// <param name="spectrum"></param>
        /// <param name="filtered"></param>
        public static void ApplyAndLog(float[][] filterbank, float[] spectrum, float[] filtered)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                filtered[i] = 0.0f;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    filtered[i] += filterbank[i][j] * spectrum[j];
                }

                filtered[i] = (float)Math.Log10(filtered[i] + float.Epsilon);
            }
        }
    }
}
