using System;
using System.Linq;
using System.Numerics;
using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Static class with methods providing general shapes of filters:
    /// 
    ///     - triangular
    ///     - rectangular
    ///     - FIR bandpass (close to trapezoidal)
    /// 
    /// ...and methods for obtaining the most widely used filterbanks:
    /// 
    ///     - Herz filterbank
    ///     - Mel filterbank
    ///     - Bark filterbank
    ///     - Critical bands
    ///     - ERB filterbank
    /// 
    /// </summary>
    public static class FilterBanks
    {
        /// <summary>
        /// General method returning universal triangular filterbank based on positions of center frequencies
        /// </summary>
        /// <param name="length">Length of each filter frequency response</param>
        /// <param name="frequencyPoints">Positions of center frequencies (including the leftmost and rightmost ones)</param>
        /// <returns>Array of triangular filters</returns>
        public static double[][] Triangular(int length, int[] frequencyPoints)
        {
            // ignore the leftmost and rightmost frequency positions
            var filterCount = frequencyPoints.Length - 2;
            
            var filterBank = new double[filterCount][];

            var leftSample = frequencyPoints[0];
            var centerSample = frequencyPoints[1];

            for (var i = 0; i < filterCount; i++)
            {
                var rightSample = frequencyPoints[i + 2];

                filterBank[i] = new double[length];

                for (var j = leftSample; j < centerSample; j++)
                {
                    filterBank[i][j] = (double)(j - leftSample) / (centerSample - leftSample);
                }
                for (var j = centerSample; j < rightSample; j++)
                {
                    filterBank[i][j] = (double)(rightSample - j) / (rightSample - centerSample);
                }

                leftSample = centerSample;
                centerSample = rightSample;
            }

            return filterBank;
        }

        /// <summary>
        /// General method returning universal rectangular filterbank based on positions of range frequencies.
        /// This filterbank is non-overlapping, so each frequency band is not described by three frequencies
        /// (left, center, right) but two frequencies (left and right).
        /// </summary>
        /// <param name="length">Length of each filter frequency response</param>
        /// <param name="frequencyPoints">Positions of frequencies (including the rightmost one)</param>
        /// <returns>Array of rectangular filters</returns>
        public static double[][] Rectangular(int length, int[] frequencyPoints)
        {
            // ignore the rightmost frequency position
            var filterCount = frequencyPoints.Length - 1;

            var filterBank = new double[filterCount][];

            var leftSample = frequencyPoints[0];

            for (var i = 0; i < filterCount; i++)
            {
                var rightSample = frequencyPoints[i + 1];

                filterBank[i] = new double[length];

                for (var j = leftSample; j < rightSample; j++)
                {
                    filterBank[i][j] = 1;
                }

                leftSample = rightSample;
            }

            return filterBank;
        }

        /// <summary>
        /// General method returning FIR bandpass (close to trapezoidal) filterbank based on positions of range frequencies.
        /// </summary>
        /// <param name="length">Length of each filter frequency response</param>
        /// <param name="frequencyPoints">Positions of frequencies (including the rightmost one)</param>
        /// <returns>Array of trapezoidal FIR filters</returns>
        public static double[][] Trapezoidal(int length, int[] frequencyPoints)
        {
            var filterBank = Rectangular(length, frequencyPoints);

            for (var i = 0; i < filterBank.Length; i++)
            {
                var filter = FilterDesign.DesignFirFilter(length, filterBank[i]);
                var filterResponse = filter.FrequencyResponse(2 * (length - 1)).Magnitude;
                filterBank[i] = filterResponse.Take(length).ToArray();

                // normalize gain to 1.0

                var maxAmp = 0.0;
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
        /// Method creates rectangular herz filters of equal width and constant height = 1
        /// </summary>
        /// <param name="combFilterCount">Number of filters</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <returns>Array of rectangular herz filters</returns>
        public static double[][] Herz(int combFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var herzResolution = (double)samplingRate / fftSize;
            var bandSize = (highFreq - lowFreq) / combFilterCount;

            var frequencyPositions = Enumerable.Range(0, combFilterCount + 1)
                                               .Select(f => (int)((lowFreq + bandSize * f) / herzResolution))
                                               .ToArray();

            return Trapezoidal(fftSize / 2 + 1, frequencyPositions);
        }
        
        /// <summary>
        /// Method creates triangular overlapping mel filters of constant height = 1
        /// </summary>
        /// <param name="melFilterCount">Number of mel filters to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <returns>Array of mel filters</returns>
        public static double[][] Mel(int melFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var herzResolution = (double)samplingRate / fftSize;
            var melResolution = (HerzToMel(highFreq) - HerzToMel(lowFreq)) / (melFilterCount + 1);

            var startingFrequency = HerzToMel(lowFreq);
            var frequencyPositions = 
                Enumerable.Range(0, melFilterCount + 2)
                          .Select(i => (int)(MelToHerz(startingFrequency + i * melResolution) / herzResolution))
                          .ToArray();
            
            return Triangular(fftSize / 2 + 1, frequencyPositions);
        }

        /// <summary>
        /// Method creates triangular overlapping bark filters of constant height = 1
        /// </summary>
        /// <param name="barkFilterCount">Number of bark filters to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <returns>Array of bark filters</returns>
        public static double[][] Bark(int barkFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0)
        {
            if (lowFreq < 0)
            {
                lowFreq = 0;
            }
            if (highFreq <= lowFreq)
            {
                highFreq = samplingRate / 2.0;
            }

            var herzResolution = (double)samplingRate / fftSize;
            var melResolution = (HerzToBark(highFreq) - HerzToBark(lowFreq)) / (barkFilterCount + 1);

            var startingFrequency = HerzToBark(lowFreq);
            var frequencyPositions =
                Enumerable.Range(0, barkFilterCount + 2)
                          .Select(i => (int)(BarkToHerz(startingFrequency + i * melResolution) / herzResolution))
                          .ToArray();

            return Triangular(fftSize / 2 + 1, frequencyPositions);
        }

        /// <summary>
        /// Method fills arrays of critical band central and edge frequencies in given range
        /// and returns total number of filters that could be used inside the given frequency range.
        /// </summary>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="samplingRate"></param>
        /// <param name="centers"></param>
        /// <param name="edges"></param>
        /// <returns></returns>
        public static int CriticalBandFrequencies(double lowFreq, double highFreq, double samplingRate, out double[] centers, out double[] edges)
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
                if (centerFrequencies[i] >= lowFreq)
                {
                    startIndex = i;
                    break;
                }
            }
            var endIndex = 0;
            for (var i = centerFrequencies.Length - 1; i >= 0; i--)
            {
                if (centerFrequencies[i] <= highFreq)
                {
                    endIndex = i;
                    break;
                }
            }

            var filterCount = endIndex - startIndex + 1;

            edges = edgeFrequencies.Skip(startIndex)
                                   .Take(filterCount + 1)
                                   .ToArray();

            centers = centerFrequencies.Skip(startIndex)
                                       .Take(filterCount + 1)
                                       .ToArray();

            return filterCount;
        }

        /// <summary>
        /// Method creates trapezoidal bandpass (not very much overlapping) critical band filters.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <returns>Array of rectangular critical band filters</returns>
        public static double[][] CriticalBands(int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0)
        {
            double[] edgeFrequencies, centerFrequencies;

            CriticalBandFrequencies(lowFreq, highFreq, samplingRate, out centerFrequencies, out edgeFrequencies);

            var herzResolution = (double)samplingRate / fftSize;
            var frequencies = edgeFrequencies.Select(f => (int)Math.Floor(f / herzResolution)).ToArray();
            return Trapezoidal(fftSize / 2 + 1, frequencies);
        }

        /// <summary>
        /// Method creates BiQuad bandpass overlapping critical band filters
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="filterQ">Q-value of each filter</param>
        /// <returns>Array of BiQuad critical band filters</returns>
        public static double[][] CriticalBandsBiQuad(int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, double filterQ = 2.0)
        {
            double[] edgeFrequencies, centerFrequencies;

            var filterCount = CriticalBandFrequencies(lowFreq, highFreq, samplingRate,
                                                      out centerFrequencies, out edgeFrequencies);

            var filterBank = new double[filterCount][];

            var halfLn2 = Math.Log(2) / 2;

            for (var i = 0; i < filterCount; i++)
            {
                var freq = centerFrequencies[i] / samplingRate;
                var q = filterQ;
                if (filterQ <= 0)
                {
                    var omega = 2 * Math.PI * freq;
                    var bw = (edgeFrequencies[i + 1] - edgeFrequencies[i]) / samplingRate * 2 * Math.PI;
                    q = 1 / (2 * Math.Sinh(halfLn2 * bw * omega / Math.Sin(omega)));
                }
                var filter = new BandPassFilter(freq, q);
                var filterResponse = filter.FrequencyResponse(fftSize).Magnitude;
                filterBank[i] = filterResponse.Take(fftSize / 2 + 1).ToArray();
            }

            return filterBank;
        }

        /// <summary>
        /// Method creates overlapping ERB filters
        /// (ported from Malcolm Slaney's MATLAB code).
        /// </summary>
        /// <param name="erbFilterCount">Number of ERB filters</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <param name="normalizeGain">True if gain should be normalized; false if all filters should have same height 1.0</param>
        /// <returns>Array of ERB filters</returns>
        public static double[][] Erb(int erbFilterCount, int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, bool normalizeGain = true)
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

            
            var erbFilterBank = new double[erbFilterCount][];
            
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

                var ir = new DiscreteSignal(1, fftSize) { [0] = 1.0 };

                var filter1 = new IirFilter(new[] { a0, a11, a2 }, new[] { b0, b1, b2 });
                var filter2 = new IirFilter(new[] { a0, a12, a2 }, new[] { b0, b1, b2 });
                var filter3 = new IirFilter(new[] { a0, a13, a2 }, new[] { b0, b1, b2 });
                var filter4 = new IirFilter(new[] { a0, a14, a2 }, new[] { b0, b1, b2 });

                var filter = filter1 * filter2 * filter3 * filter4;

                ir = filter.ApplyTo(ir);

                for (var j = 0; j < ir.Length; j++)
                {
                    ir.Samples[j] = ir[j] / gain;
                }

                var fft = new Fft(fftSize);
                erbFilterBank[i] = fft.PowerSpectrum(ir, false).Samples;
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
                    sum += Math.Abs(filter[j]*filter[j]);
                }

                var weight = Math.Sqrt(sum*samplingRate/fftSize);

                for (var j = 0; j < filter.Length; j++)
                {
                    filter[j] /= weight;
                }
            }

            return erbFilterBank;
        }

        /// <summary>
        /// Method converts herz frequency to corresponding mel frequency
        /// </summary>
        /// <param name="herz">Herz frequency</param>
        /// <returns>Mel frequency</returns>
        public static double HerzToMel(double herz)
        {
            return 1127.01048 * Math.Log(herz / 700 + 1);
        }

        /// <summary>
        /// Method converts mel frequency to corresponding herz frequency
        /// </summary>
        /// <param name="mel">Mel frequency</param>
        /// <returns>Herz frequency</returns>
        public static double MelToHerz(double mel)
        {
            return (Math.Exp(mel / 1127.01048) - 1) * 700;
        }

        /// <summary>
        /// Method converts herz frequency to corresponding bark frequency
        /// (according to Traunmüller (1990))
        /// </summary>
        /// <param name="herz">Herz frequency</param>
        /// <returns>Bark frequency</returns>
        public static double HerzToBark(double herz)
        {
            return (26.81 * herz) / (1960 + herz) - 0.53;
        }

        /// <summary>
        /// Method converts bark frequency to corresponding herz frequency
        /// (according to Traunmüller (1990))
        /// </summary>
        /// <param name="bark">Bark frequency</param>
        /// <returns>Herz frequency</returns>
        public static double BarkToHerz(double bark)
        {
            return 1960 / (26.81 / (bark + 0.53) - 1);
        }

        /// <summary>
        /// Method applies filters to spectrum and fills resulting filtered spectrum.
        /// </summary>
        /// <param name="filterbank"></param>
        /// <param name="spectrum"></param>
        /// <param name="filtered"></param>
        public static void Apply(double[][] filterbank, double[] spectrum, double[] filtered)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                filtered[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    filtered[i] += filterbank[i][j] * spectrum[j];
                }
            }
        }
        
        /// <summary>
        /// Method applies filters to spectrum and then does Log10() on resulting spectrum.
        /// </summary>
        /// <param name="filterbank"></param>
        /// <param name="spectrum"></param>
        /// <param name="filtered"></param>
        public static void ApplyAndLog(double[][] filterbank, double[] spectrum, double[] filtered)
        {
            for (var i = 0; i < filterbank.Length; i++)
            {
                filtered[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    filtered[i] += filterbank[i][j] * spectrum[j];
                }

                filtered[i] = Math.Log10(filtered[i] + double.Epsilon);
            }
        }
    }
}
