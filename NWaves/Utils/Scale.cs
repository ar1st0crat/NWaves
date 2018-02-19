using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Static class providing methods for converting between different scales:
    /// 
    ///     - decibel
    ///     - mel
    ///     - bark
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
            return 20 * Math.Log10(value / valueReference);
        }

        /// <summary>
        /// Method converts power to dB level
        /// </summary>
        /// <param name="value">Power</param>
        /// <param name="valueReference">Reference power</param>
        /// <returns>Decibel level</returns>
        public static double ToDecibelPower(double value, double valueReference = 1.0)
        {
            return 10 * Math.Log10(value / valueReference);
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
