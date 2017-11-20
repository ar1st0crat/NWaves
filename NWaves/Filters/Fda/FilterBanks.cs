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
    /// Static class providing methods for obtaining the most widely used filterbanks:
    /// 
    ///     - Fourier (rectangular) filterbank
    ///     - Mel (triangular) filterbank
    ///     - Bark (triangular) filterbank
    ///     - Critical bands BiQuad (Bark bandpass) filters
    ///     - Critical bands trapezoidal (Bark bandpass) filters
    ///     - ERB filterbank
    ///     - Equal Loudness Curves
    /// 
    /// </summary>
    public static class FilterBanks
    {
        /// <summary>
        /// General method returning universal triangular filterbank based on positions of center frequencies
        /// </summary>
        /// <param name="filterCount">Number of filters to create</param>
        /// <param name="length">Length of each filter's frequency response</param>
        /// <param name="frequencyPoints">Positions of center frequencies</param>
        /// <returns>Array of triangular filters</returns>
        public static double[][] Triangular(int filterCount, int length, int[] frequencyPoints)
        {
            var filterBanks = new double[filterCount][];

            var leftSample = frequencyPoints[0];
            var centerSample = frequencyPoints[1];

            for (var i = 0; i < filterCount; i++)
            {
                var rightSample = frequencyPoints[i + 2];

                filterBanks[i] = new double[length];

                for (var j = leftSample; j < centerSample; j++)
                {
                    filterBanks[i][j] = (double)(j - leftSample) / (centerSample - leftSample);
                }
                for (var j = centerSample; j < rightSample; j++)
                {
                    filterBanks[i][j] = (double)(rightSample - j) / (rightSample - centerSample);
                }

                leftSample = centerSample;
                centerSample = rightSample;
            }

            return filterBanks;
        }

        /// <summary>
        /// General method returning universal rectangular filterbank based on positions of center frequencies
        /// </summary>
        /// <param name="filterCount">Number of filters to create</param>
        /// <param name="length">Length of each filter's frequency response</param>
        /// <param name="frequencyPoints">Positions of center frequencies</param>
        /// <returns>Array of rectangular filters</returns>
        public static double[][] Rectangular(int filterCount, int length, int[] frequencyPoints)
        {
            var filterBanks = new double[filterCount][];

            var leftSample = frequencyPoints[0];
            
            for (var i = 0; i < filterCount; i++)
            {
                var rightSample = frequencyPoints[i + 1];

                filterBanks[i] = new double[length];

                for (var j = leftSample; j < rightSample; j++)
                {
                    filterBanks[i][j] = 1;
                }

                leftSample = rightSample;
            }

            return filterBanks;
        }

        /// <summary>
        /// Method creates rectangular Fourier filters of equal width and constant height = 1
        /// </summary>
        /// <param name="combFilterCount">Number of filters</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <returns>Array of rectangular Fourier filters</returns>
        public static double[][] Fourier(int combFilterCount, int fftSize)
        {
            var size = fftSize / 2 + 1;
            var bandSize = (double)size / combFilterCount;

            var frequencyPositions = Enumerable.Range(0, combFilterCount + 1)
                                               .Select(f => (int)(bandSize * f))
                                               .ToArray();
            
            return Rectangular(combFilterCount, size, frequencyPositions);
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
                          .Select(i => (int)Math.Floor((MelToHerz(startingFrequency + i * melResolution)) / herzResolution))
                          .ToArray();

            return Triangular(melFilterCount, fftSize / 2 + 1, frequencyPositions);
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
                          .Select(i => (int)Math.Floor((BarkToHerz(startingFrequency + i * melResolution)) / herzResolution))
                          .ToArray();

            return Triangular(barkFilterCount, fftSize / 2 + 1, frequencyPositions);
        }

        /// <summary>
        /// Method yields set of critical band central and edge frequencies in given range
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
                                   .Take(filterCount + 2)
                                   .ToArray();

            centers = centerFrequencies.Skip(startIndex)
                                       .Take(filterCount + 1)
                                       .ToArray();

            return filterCount;
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
        public static double[][] CriticalBands(int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0, double filterQ = 2.0)
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
        /// Method creates rectangular (in fact closer to trapezoidal) 
        /// bandpass (not very much overlapping) critical band filters.
        /// </summary>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <param name="lowFreq">Lower bound of the frequency range</param>
        /// <param name="highFreq">Upper bound of the frequency range</param>
        /// <returns>Array of rectangular critical band filters</returns>
        public static double[][] CriticalBandsRectangular(int fftSize, int samplingRate, double lowFreq = 0, double highFreq = 0)
        {
            double[] edgeFrequencies, centerFrequencies;

            var filterCount = CriticalBandFrequencies(lowFreq, highFreq, samplingRate, 
                                                      out centerFrequencies, out edgeFrequencies);

            var herzResolution = (double)samplingRate / fftSize;
            var frequencies = edgeFrequencies.Select(f => (int)Math.Floor(f / herzResolution)).ToArray();
            var filterBank = Rectangular(filterCount, fftSize / 2 + 1, frequencies);

            for (var i = 0; i < filterCount; i++)
            {
                var filter = FilterDesign.DesignFirFilter(fftSize / 4 - 1, filterBank[i]);
                var filterResponse = filter.FrequencyResponse(fftSize).Magnitude;
                filterBank[i] = filterResponse.Take(fftSize / 2 + 1).ToArray();

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

                var ir = new DiscreteSignal(1, fftSize * 2) { [0] = 1.0 };

                var filter1 = new IirFilter(new[] { a0, a11, a2 }, new[] { b0, b1, b2 });
                var filter2 = new IirFilter(new[] { a0, a12, a2 }, new[] { b0, b1, b2 });
                var filter3 = new IirFilter(new[] { a0, a13, a2 }, new[] { b0, b1, b2 });
                var filter4 = new IirFilter(new[] { a0, a14, a2 }, new[] { b0, b1, b2 });

                ir = filter1.ApplyTo(ir);
                ir = filter2.ApplyTo(ir);
                ir = filter3.ApplyTo(ir);
                ir = filter4.ApplyTo(ir);

                for (var j = 0; j < ir.Length; j++)
                {
                    ir.Samples[j] = ir[j] / gain;
                }

                erbFilterBank[i] = Transform.PowerSpectrum(ir.Samples, fftSize, false);
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


        //public static double[] EqualLoudnessCurve(double phon)
        //{
        /*

        function [spl, freq] = iso226(phon);
        %
        % Generates an Equal Loudness Contour as described in ISO 226
        %
        % Usage:  [SPL FREQ] = ISO226(PHON);
        % 
        %         PHON is the phon value in dB SPL that you want the equal
        %           loudness curve to represent. (1phon = 1dB @ 1kHz)
        %         SPL is the Sound Pressure Level amplitude returned for
        %           each of the 29 frequencies evaluated by ISO226.
        %         FREQ is the returned vector of frequencies that ISO226
        %           evaluates to generate the contour.
        %
        % Desc:   This function will return the equal loudness contour for
        %         your desired phon level.  The frequencies evaulated in this
        %         function only span from 20Hz - 12.5kHz, and only 29 selective
        %         frequencies are covered.  This is the limitation of the ISO
        %         standard.
        %
        %         In addition the valid phon range should be 0 - 90 dB SPL.
        %         Values outside this range do not have experimental values
        %         and their contours should be treated as inaccurate.
        %
        %         If more samples are required you should be able to easily
        %         interpolate these values using spline().
        %
        % Author: sparafucile17 03/01/05

        %                /---------------------------------------\
        %%%%%%%%%%%%%%%%%          TABLES FROM ISO226             %%%%%%%%%%%%%%%%%
        %                \---------------------------------------/
        f = [20 25 31.5 40 50 63 80 100 125 160 200 250 315 400 500 630 800 ...
             1000 1250 1600 2000 2500 3150 4000 5000 6300 8000 10000 12500];

        af = [0.532 0.506 0.480 0.455 0.432 0.409 0.387 0.367 0.349 0.330 0.315 ...
              0.301 0.288 0.276 0.267 0.259 0.253 0.250 0.246 0.244 0.243 0.243 ...
              0.243 0.242 0.242 0.245 0.254 0.271 0.301];

        Lu = [-31.6 -27.2 -23.0 -19.1 -15.9 -13.0 -10.3 -8.1 -6.2 -4.5 -3.1 ...
               -2.0  -1.1  -0.4   0.0   0.3   0.5   0.0 -2.7 -4.1 -1.0  1.7 ...
                2.5   1.2  -2.1  -7.1 -11.2 -10.7  -3.1];

        Tf = [ 78.5  68.7  59.5  51.1  44.0  37.5  31.5  26.5  22.1  17.9  14.4 ...
               11.4   8.6   6.2   4.4   3.0   2.2   2.4   3.5   1.7  -1.3  -4.2 ...
               -6.0  -5.4  -1.5   6.0  12.6  13.9  12.3];
        %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%    

        %Error Trapping
        if((phon < 0) | (phon > 90))
            disp('Phon value out of bounds!')
            spl = 0;
            freq = 0;
        else
            %Setup user-defined values for equation
            Ln = phon;

            %Deriving sound pressure level from loudness level (iso226 sect 4.1)
            Af=4.47E-3 * (10.^(0.025*Ln) - 1.15) + (0.4*10.^(((Tf+Lu)/10)-9 )).^af;
            Lp=((10./af).*log10(Af)) - Lu + 94;

            %Return user data
            spl = Lp;  
            freq = f;
        end

        */
        //}

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
    }
}
