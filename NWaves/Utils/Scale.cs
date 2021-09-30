using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Provides methods for 
    /// <para>
    /// 1) converting between different scales:
    /// <list type="bullet">
    ///     <item>decibel</item>
    ///     <item>MIDI pitch</item>
    ///     <item>mel (HTK)</item>
    ///     <item>mel (Slaney)</item>
    ///     <item>bark1 (Traunmueller)</item>
    ///     <item>bark2 (Wang)</item>
    ///     <item>ERB</item>
    /// </list>
    /// </para>
    /// <para>
    /// 2) loudness weighting:
    /// <list>
    ///     <item>A-weighting</item>
    ///     <item>B-weighting</item>
    ///     <item>C-weighting</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class Scale
    {
        /// <summary>
        /// Converts magnitude value to dB level.
        /// </summary>
        /// <param name="value">Magnitude</param>
        /// <param name="valueReference">Reference magnitude</param>
        public static double ToDecibel(double value, double valueReference)
        {
            return 20 * Math.Log10(value / valueReference + double.Epsilon);
        }

        /// <summary>
        /// Converts magnitude value to dB level (simplified version).
        /// </summary>
        /// <param name="value">Magnitude</param>
        public static double ToDecibel(double value)
        {
            return 20 * Math.Log10(value);
        }

        /// <summary>
        /// Converts power to dB level.
        /// </summary>
        /// <param name="value">Power</param>
        /// <param name="valueReference">Reference power</param>
        public static double ToDecibelPower(double value, double valueReference = 1.0)
        {
            return 10 * Math.Log10(value / valueReference + double.Epsilon);
        }

        /// <summary>
        /// Converts dB level to magnitude value.
        /// </summary>
        /// <param name="level">Decibel level</param>
        /// <param name="valueReference">Reference magnitude</param>
        public static double FromDecibel(double level, double valueReference)
        {
            return valueReference * Math.Pow(10, level / 20);
        }

        /// <summary>
        /// Converts dB level to magnitude value (simplified version).
        /// </summary>
        /// <param name="level">Decibel level</param>
        public static double FromDecibel(double level)
        {
            return Math.Pow(10, level / 20);
        }

        /// <summary>
        /// Converts dB level to power.
        /// </summary>
        /// <param name="level">Decibel level</param>
        /// <param name="valueReference">Reference power</param>
        public static double FromDecibelPower(double level, double valueReference = 1.0)
        {
            return valueReference * Math.Pow(10, level / 10);
        }

        /// <summary>
        /// Converts MIDI pitch to frequency (in Hz).
        /// </summary>
        /// <param name="pitch">Pitch</param>
        public static double PitchToFreq(int pitch)
        {
            return 440 * Math.Pow(2, (pitch - 69) / 12.0);
        }

        /// <summary>
        /// Converts frequency to MIDI pitch.
        /// </summary>
        /// <param name="freq">Frequency (in Hz)</param>
        public static int FreqToPitch(double freq)
        {
            return (int)Math.Round(69 + 12 * Math.Log(freq / 440, 2), MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Array of musical notes.
        /// </summary>
        public static string[] Notes = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        /// <summary>
        /// Converts musical note (in format ("G", 3), ("E", 5), etc.) to frequency in Hz.
        /// </summary>
        /// <param name="note">Note (A-G#)</param>
        /// <param name="octave">Octave (0-8)</param>
        public static double NoteToFreq(string note, int octave)
        {
            var noteIndex = Array.IndexOf(Notes, note);

            if (noteIndex < 0)
            {
                throw new ArgumentException("Incorrect note. Valid notes are: " + string.Join(", ", Notes));
            }

            if (octave < 0 || octave > 8)
            {
                throw new ArgumentException("Incorrect octave. Valid octave range is [0, 8]");
            }

            return PitchToFreq(noteIndex + 12 * (octave + 1));
        }

        /// <summary>
        /// Converts frequency in Hz to note (in tuple format ("G", 3), ("E", 5), etc.).
        /// </summary>
        /// <param name="freq">Frequency in Hz</param>
        public static (string, int) FreqToNote(double freq)
        {
            var pitch = FreqToPitch(freq);

            var note = Notes[pitch % 12];
            var octave = pitch / 12 - 1;

            return (note, octave);
        }

        /// <summary>
        /// Converts herz frequency to corresponding mel frequency.
        /// </summary>
        public static double HerzToMel(double herz)
        {
            return 1127 * Math.Log(herz / 700 + 1); // actually, should be 1127.01048, but HTK and Kaldi seem to use 1127
        }

        /// <summary>
        /// Converts mel frequency to corresponding herz frequency.
        /// </summary>
        public static double MelToHerz(double mel)
        {
            return (Math.Exp(mel / 1127) - 1) * 700;
        }

        /// <summary>
        /// Converts herz frequency to mel frequency (suggested by M.Slaney).
        /// </summary>
        public static double HerzToMelSlaney(double herz)
        {
            const double minHerz = 0.0;
            const double sp = 200.0 / 3;
            const double minLogHerz = 1000.0;
            const double minLogMel = (minLogHerz - minHerz) / sp;

            var logStep = Math.Log(6.4) / 27;

            return herz < minLogHerz ? (herz - minHerz) / sp : minLogMel + Math.Log(herz / minLogHerz) / logStep;
        }

        /// <summary>
        /// Converts mel frequency to herz frequency (suggested by M.Slaney).
        /// </summary>
        public static double MelToHerzSlaney(double mel)
        {
            const double minHerz = 0.0;
            const double sp = 200.0 / 3;
            const double minLogHerz = 1000.0;
            const double minLogMel = (minLogHerz - minHerz) / sp;

            var logStep = Math.Log(6.4) / 27;

            return mel < minLogMel ? minHerz + sp * mel : minLogHerz * Math.Exp(logStep * (mel - minLogMel));
        }

        /// <summary>
        /// Converts herz frequency to corresponding bark frequency (according to Traunmüller (1990)).
        /// </summary>
        public static double HerzToBark(double herz)
        {
            return (26.81 * herz) / (1960 + herz) - 0.53;
        }

        /// <summary>
        /// Converts bark frequency to corresponding herz frequency (according to Traunmüller (1990)).
        /// </summary>
        public static double BarkToHerz(double bark)
        {
            return 1960 / (26.81 / (bark + 0.53) - 1);
        }

        /// <summary>
        /// Converts herz frequency to corresponding bark frequency (according to Wang (1992)); 
        /// used in M.Slaney's auditory toolbox.
        /// </summary>
        public static double HerzToBarkSlaney(double herz)
        {
            return 6 * MathUtils.Asinh(herz / 600);
        }

        /// <summary>
        /// Converts bark frequency to corresponding herz frequency (according to Wang (1992)); 
        /// used in M.Slaney's auditory toolbox.
        /// </summary>
        public static double BarkToHerzSlaney(double bark)
        {
            return 600 * Math.Sinh(bark / 6);
        }

        /// <summary>
        /// Converts herz frequency to corresponding ERB frequency.
        /// </summary>
        public static double HerzToErb(double herz)
        {
            return 9.26449 * Math.Log(1.0 + herz) / (24.7 * 9.26449);
        }

        /// <summary>
        /// Converts ERB frequency to corresponding herz frequency.
        /// </summary>
        public static double ErbToHerz(double erb)
        {
            return (Math.Exp(erb / 9.26449) - 1.0) * (24.7 * 9.26449);
        }

        /// <summary>
        /// Converts Hz frequency to octave (used for constructing librosa-like Chroma filterbanks).
        /// </summary>
        public static double HerzToOctave(double herz, double tuning = 0, int binsPerOctave = 12)
        {
            var a440 = 440.0 * Math.Pow(2.0, tuning / binsPerOctave);

            return Math.Log(16 * herz / a440, 2);
        }

        /// <summary>
        /// Returns perceptual loudness weight (in dB).
        /// </summary>
        /// <param name="frequency">Frequency</param>
        /// <param name="weightingType">Weighting type (A, B, C)</param>
        public static double LoudnessWeighting(double frequency, string weightingType = "A")
        {
            var level2 = frequency * frequency;

            switch (weightingType.ToUpper())
            {
                case "B":
                {
                    var r = (level2 * frequency * 148693636) /
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
