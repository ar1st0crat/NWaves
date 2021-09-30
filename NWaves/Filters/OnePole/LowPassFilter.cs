using System;

namespace NWaves.Filters.OnePole
{
    /// <summary>
    /// Represents one-pole lowpass filter.
    /// </summary>
    public class LowPassFilter : OnePoleFilter
    {
        /// <summary>
        /// Gets cutoff frequency.
        /// </summary>
        public double Frequency { get; protected set; }

        /// <summary>
        /// Constructs <see cref="LowPassFilter"/> with given cutoff <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">Cutoff frequency</param>
        public LowPassFilter(double frequency)
        {
            SetCoefficients(frequency);
        }

        /// <summary>
        /// Sets filter coefficients based on given cutoff <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">Cutoff frequency</param>
        private void SetCoefficients(double frequency)
        {
            Frequency = frequency;

            _a[0] = 1;
            _a[1] = (float)(-Math.Exp(-2 * Math.PI * frequency));

            _b[0] = 1 + _a[1];
        }

        /// <summary>
        /// Changes filter coefficients (preserving the state of the filter).
        /// </summary>
        /// <param name="frequency">Cutoff frequency</param>
        public void Change(double frequency)
        {
            SetCoefficients(frequency);
        }
    }
}
