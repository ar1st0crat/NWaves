using System;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// Represents BiQuad low-shelving filter.
    /// </summary>
    public class LowShelfFilter : BiQuadFilter
    {
        /// <summary>
        /// Gets shelf midpoint frequency.
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
        /// Constructs <see cref="LowShelfFilter"/>.
        /// </summary>
        /// <param name="freq">Shelf midpoint frequency</param>
        /// <param name="q">Q factor</param>
        /// <param name="gain">Gain (in dB)</param>
        public LowShelfFilter(double freq, double q = 1, double gain = 1.0)
        {
            SetCoefficients(freq, q, gain);
        }

        /// <summary>
        /// Sets filter coefficients.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="q">Q factor</param>
        /// <param name="gain">Gain (in dB)</param>
        private void SetCoefficients(double freq, double q, double gain)
        {
            // The coefficients are calculated according to 
            // audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.

            Freq = freq;
            Q = q;
            Gain = gain;

            var ga = Math.Pow(10, gain / 40);
            var asqrt = Math.Sqrt(ga);
            var omega = 2 * Math.PI * freq;
            var alpha = Math.Sin(omega) / 2 * Math.Sqrt((ga + 1 / ga) * (1 / q - 1) + 2);
            var cosw = Math.Cos(omega);

            _b[0] = (float) (ga * (ga + 1 - (ga - 1) * cosw + 2 * asqrt * alpha));
            _b[1] = (float) (2 * ga * (ga - 1 - (ga + 1) * cosw));
            _b[2] = (float) (ga * (ga + 1 - (ga - 1) * cosw - 2 * asqrt * alpha));

            _a[0] = (float) (ga + 1 + (ga - 1) * cosw + 2 * asqrt * alpha);
            _a[1] = (float) (-2 * (ga - 1 + (ga + 1) * cosw));
            _a[2] = (float) (ga + 1 + (ga - 1) * cosw - 2 * asqrt * alpha);

            Normalize();
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="q">Q factor</param>
        /// <param name="gain">Gain (in dB)</param>
        public void Change(double freq, double q = 1, double gain = 1.0)
        {
            SetCoefficients(freq, q, gain);
        }
    }
}