using System;

namespace NWaves.Filters
{
    /// <summary>
    /// Static class providing methods for obtaining the most widely used filterbanks:
    /// 
    ///     - Fourier (rectangular) filterbanks
    ///     - Mel (triangular) filterbanks
    ///     - Bark (triangular) filterbanks
    ///     - ERB filterbanks
    ///     - Equal Loudness Curves
    /// 
    /// </summary>
    public static class FilterBanks
    {
        /// <summary>
        /// Method creates rectangular Fourier filterbanks of constant height = 1
        /// </summary>
        /// <param name="combFilterCount"></param>
        /// <param name="fftSize"></param>
        /// <returns></returns>
        public static double[][] Fourier(int combFilterCount, int fftSize)
        {
            var size = fftSize / 2;
            var bandSize = (double)size / combFilterCount;
            var filterBanks = new double[combFilterCount][];

            for (var i = 0; i < combFilterCount; i++)
            {
                filterBanks[i] = new double[size];
                for (var j = (int)(bandSize * i); j < (int)(bandSize * (i + 1)); j++)
                {
                    filterBanks[i][j] = 1;
                }
            }

            return filterBanks;
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
        /// Method creates triangular mel filterbanks of constant height = 1
        /// </summary>
        /// <param name="melFilterCount">Number of mel filterbanks to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <returns></returns>
        public static double[][] Mel(int melFilterCount, int fftSize, int samplingRate)
        {
            var melFilterBanks = new double[melFilterCount][];

            var frequencyResolution = HerzToMel(samplingRate / 2.0) / (melFilterCount + 1);

            var leftSample = 0;

            var melFrequency = frequencyResolution;
            var centerSample = (int)Math.Floor((fftSize + 1) * MelToHerz(melFrequency) / samplingRate);

            for (var i = 0; i < melFilterCount; i++)
            {
                melFrequency += frequencyResolution;

                var rightSample = (int)Math.Floor((fftSize + 1) * MelToHerz(melFrequency) / samplingRate);

                melFilterBanks[i] = new double[fftSize / 2];

                for (var j = leftSample; j < centerSample; j++)
                {
                    melFilterBanks[i][j] = (double)(j - leftSample) / (centerSample - leftSample);
                }
                for (var j = centerSample; j < rightSample; j++)
                {
                    melFilterBanks[i][j] = (double)(rightSample - j) / (rightSample - centerSample);
                }

                leftSample = centerSample;
                centerSample = rightSample;
            }

            return melFilterBanks;
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
        /// 
        /// </summary>
        public const int MaxBarkFilters = 25;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="barkFilterCount">Number of bark filterbanks to create</param>
        /// <param name="fftSize">Assumed size of FFT</param>
        /// <param name="samplingRate">Assumed sampling rate of a signal</param>
        /// <returns></returns>
        public static double[][] Bark(int barkFilterCount, int fftSize, int samplingRate)
        {
            if (barkFilterCount > MaxBarkFilters)
            {
                throw new ArgumentException(string.Format("Maximum {0} bark filters are supported", MaxBarkFilters));
            }

            double[] edgeFrequencies = { 0,    100,  200,  300,  400,  510,  630,  770,  920,  1080, 1270,  1480,  1720,
                                         2000, 2320, 2700, 3150, 3700, 4400, 5300, 6400, 7700, 9500, 12000, 15500, 20500 };

            double[] centerFrequencies = { 50,   150,  250,  350,  450,  570,  700,  840,  1000, 1170, 1370,  1600,
                                           1850, 2150, 2500, 2900, 3400, 4000, 4800, 5800, 7000, 8500, 10500, 13500, 17500 };

            var barkFilterBanks = new double[barkFilterCount][];

            var leftSample = 0;
            
            for (var i = 0; i < barkFilterCount; i++)
            {
                barkFilterBanks[i] = new double[fftSize / 2];

                var centerSample = (int)Math.Floor((fftSize + 1) * centerFrequencies[i] / samplingRate);
                var rightSample = (int)Math.Floor((fftSize + 1) * edgeFrequencies[i + 1] / samplingRate);

                for (var j = leftSample; j < centerSample; j++)
                {
                    barkFilterBanks[i][j] = (double)(j - leftSample) / (centerSample - leftSample);
                }
                for (var j = centerSample; j < rightSample; j++)
                {
                    barkFilterBanks[i][j] = (double)(rightSample - j) / (rightSample - centerSample);
                }

                leftSample = rightSample;
            }

            return barkFilterBanks;
        }

        //public static double[][] Erb()
        //{

        //}

        //public static double[][] EqualLoudness()
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
    }
}
