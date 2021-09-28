using System;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// Represents BiQuad high-pass filter.
    /// </summary>
    public class HighPassFilter : BiQuadFilter
    {
        /// <summary>
        /// Gets cutoff frequency.
        /// </summary>
        public double Freq { get; protected set; }

        /// <summary>
        /// Gets Q factor.
        /// </summary>
        public double Q { get; protected set; }

        /// <summary>
        /// Constructs <see cref="HighPassFilter"/>.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="q">Q factor</param>
        public HighPassFilter(double freq, double q = 1)
        {
            SetCoefficients(freq, q);
        }

        /// <summary>
        /// Sets filter coefficients.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="q">Q factor</param>
        private void SetCoefficients(double freq, double q)
        {
            // The coefficients are calculated according to 
            // audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.

            Freq = freq;
            Q = q;

            var omega = 2 * Math.PI * freq;
            var alpha = Math.Sin(omega) / (2 * q);
            var cosw = Math.Cos(omega);

            _b[0] = (float)((1 + cosw) / 2);
            _b[1] = (float)(-(1 + cosw));
            _b[2] = _b[0];

            _a[0] = (float)(1 + alpha);
            _a[1] = (float)(-2 * cosw);
            _a[2] = (float)(1 - alpha);

            Normalize();
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="q">Q factor</param>
        public void Change(double freq, double q = 1)
        {
            SetCoefficients(freq, q);
        }
    }
}
