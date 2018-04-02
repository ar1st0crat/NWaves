using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Static class providing methods for 
    /// 
    /// 1) converting between different scales:
    ///     - decibel
    ///     - MIDI pitch
    ///     - mel
    ///     - bark
    ///     - ERB
    /// 
    /// 2) loudness weighting:
    ///     - A-weighting
    ///     - B-weighting
    ///     - C-weighting
    /// 
    /// </summary>
    public static class Scale
    {
        /// <summary>
        /// Method converts magnitude value to dB level
        /// </summary>
        /// <param name="value">Magnitude</param>
        /// <param name="valueReference">Reference magnitude</param>
        /// <returns>Decibel level</returns>
        public static double ToDecibel(double value, double valueReference = 1.0)
        {
            return 20 * Math.Log10(value / valueReference + double.Epsilon);
        }

        /// <summary>
        /// Method converts power to dB level
        /// </summary>
        /// <param name="value">Power</param>
        /// <param name="valueReference">Reference power</param>
        /// <returns>Decibel level</returns>
        public static double ToDecibelPower(double value, double valueReference = 1.0)
        {
            return 10 * Math.Log10(value / valueReference + double.Epsilon);
        }

        /// <summary>
        /// Method converts dB level to magnitude value
        /// </summary>
        /// <param name="level">dB level</param>
        /// <param name="valueReference">Reference magnitude</param>
        /// <returns>Magnitude value</returns>
        public static double FromDecibel(double level, double valueReference = 1.0)
        {
            return valueReference * Math.Pow(10, level / 20);
        }

        /// <summary>
        /// Method converts dB level to power
        /// </summary>
        /// <param name="level">dB level</param>
        /// <param name="valueReference">Reference power</param>
        /// <returns>Power</returns>
        public static double FromDecibelPower(double level, double valueReference = 1.0)
        {
            return valueReference * Math.Pow(10, level / 10);
        }

        /// <summary>
        /// Method converts MIDI pitch to frequency
        /// </summary>
        /// <param name="pitch"></param>
        /// <returns></returns>
        public static double PitchToFreq(int pitch)
        {
            return 440 * Math.Pow(2, (pitch - 69) / 12.0);
        }

        /// <summary>
        /// Method converts frequency to MIDI pitch
        /// </summary>
        /// <param name="freq"></param>
        /// <returns></returns>
        public static int FreqToPitch(double freq)
        {
            return (int)Math.Round(69 + 12 * Math.Log(freq / 440, 2), MidpointRounding.AwayFromZero);
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
        /// Method converts herz frequency to corresponding ERB frequency
        /// </summary>
        /// <param name="herz">Herz frequency</param>
        /// <returns>ERB frequency</returns>
        public static double HerzToErb(double herz)
        {
            return 9.26449 * Math.Log(1.0 + herz) / (24.7 * 9.26449);
        }

        /// <summary>
        /// Method converts ERB frequency to corresponding herz frequency
        /// </summary>
        /// <param name="erb">ERB frequency</param>
        /// <returns>Herz frequency</returns>
        public static double ErbToHerz(double erb)
        {
            return (Math.Exp(erb / 9.26449) - 1.0) * (24.7 * 9.26449);
        }

        /// <summary>
        /// Method for obtaining a perceptual loudness weight
        /// </summary>
        /// <param name="freq">Frequency</param>
        /// <param name="weightingType">Weighting type (A, B, C)</param>
        /// <returns>Weight value in dB</returns>
        public static double LoudnessWeighting(double freq, string weightingType = "A")
        {
            var level2 = freq * freq;

            switch (weightingType.ToUpper())
            {
                case "B":
                {
                    var r = (level2 * freq * 148693636) /
                             (
                                (level2 + 424.36) *
                                 Math.Sqrt(level2 + 25122.25) *
                                (level2 + 148693636)
                             );
                    return 20 * Math.Log10(r) + 0.17;
                }
                    
                case "C":
                {
                    var r = (level2 * 148693636) /
                             (
                                 (level2 + 424.36) *
                                 (level2 + 148693636)
                             );
                    return 20 * Math.Log10(r) + 0.06;
                }

                default:
                {
                    var r = (level2 * level2 * 148693636) / 
                             (
                                 (level2 + 424.36) * 
                                  Math.Sqrt((level2 + 11599.29) * (level2 + 544496.41)) * 
                                 (level2 + 148693636)
                             );
                    return 20 * Math.Log10(r) + 2.0;
                }
            }
        }
    }
}
