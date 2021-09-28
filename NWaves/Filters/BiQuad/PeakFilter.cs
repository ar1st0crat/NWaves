using System;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// Represents BiQuad peaking EQ filter.
    /// </summary>
    public class PeakFilter : BiQuadFilter
    {
        /// <summary>
        /// Gets center frequency.
        /// </summary>
        public double Freq { get; protected set; }

        /// <summary>
        /// Gets Q factor.
        /// </summary>
        public double Q { get; protected set; }

        /// <summary>
        /// Gets gain (in dB).
        /// </summary>
        public double Gain { get; protected set; }

        /// <summary>
        /// Constructs <see cref="PeakFilter"/>.
        /// </summary>
        /// <param name="freq">Center frequency</param>
        /// <param name="q">Q factor</param>
        /// <param name="gain">Gain (in dB)</param>
        public PeakFilter(double freq, double q = 1, double gain = 1.0)
        {
            SetCoefficients(freq, q, gain);
        }

        /// <summary>
        /// Sets filter coefficients.
        /// </summary>
        /// <param name="freq">Center frequency</param>
        /// <param name="q">Q factor</param>
        /// <param name="gain">Gain (in dB)</param>
        private void SetCoefficients(double freq, double q, double gain)
        {
            // The coefficients are calculated automatically according to 
            // audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.

            Freq = freq;
            Q = q;
            Gain = gain;

            var ga = Math.Pow(10, gain / 40);
            var omega = 2 * Math.PI * freq;
            var alpha = Math.Sin(omega) / (2 * q);
            var cosw = Math.Cos(omega);

            _b[0] = (float)(1 + alpha * ga);
            _b[1] = (float)(-2 * cosw);
            _b[2] = (float)(1 - alpha * ga);

            _a[0] = (float)(1 + alpha / ga);
            _a[1] = (float)(-2 * cosw);
            _a[2] = (float)(1 - alpha / ga);

            Normalize();
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq">Center frequency</param>
        /// <param name="q">Q factor</param>
        /// <param name="gain">Gain (in dB)</param>
        public void Change(double freq, double q = 1, double gain = 1.0)
        {
            SetCoefficients(freq, q, gain);
        }
    }
}
