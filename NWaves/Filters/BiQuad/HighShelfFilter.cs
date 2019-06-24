using System;

namespace NWaves.Filters.BiQuad
{
    /// <summary>
    /// BiQuad high-shelving filter.
    /// The coefficients are calculated automatically according to 
    /// audio-eq-cookbook by R.Bristow-Johnson and WebAudio API.
    /// </summary>
    public class HighShelfFilter : BiQuadFilter
    {
        /// <summary>
        /// Frequency
        /// </summary>
        public double Freq { get; protected set; }

        /// <summary>
        /// Q
        /// </summary>
        public double Q { get; protected set; }

        /// <summary>
        /// Gain
        /// </summary>
        public double Gain { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        public HighShelfFilter(double freq, double q = 1, double gain = 1.0)
        {
            SetCoefficients(freq, q, gain);
        }

        /// <summary>
        /// Set filter coefficients
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        private void SetCoefficients(double freq, double q, double gain)
        {
            Freq = freq;
            Q = q;
            Gain = gain;

            var ga = Math.Pow(10, gain / 40);
            var asqrt = Math.Sqrt(ga);
            var omega = 2 * Math.PI * freq;
            var alpha = Math.Sin(omega) / 2 * Math.Sqrt((ga + 1 / ga) * (1 / q - 1) + 2);
            var cosw = Math.Cos(omega);

            _b[0] = (float) (ga * (ga + 1 + (ga - 1) * cosw + 2 * asqrt * alpha));
            _b[1] = (float) (-2 * ga * (ga - 1 + (ga + 1) * cosw));
            _b[2] = (float) (ga * (ga + 1 + (ga - 1) * cosw - 2 * asqrt * alpha));

            _a[0] = (float) (ga + 1 - (ga - 1) * cosw + 2 * asqrt * alpha);
            _a[1] = (float) (2 * (ga - 1 - (ga + 1) * cosw));
            _a[2] = (float) (ga + 1 - (ga - 1) * cosw - 2 * asqrt * alpha);

            Normalize();
        }

        /// <summary>
        /// Change filter parameters (preserving its state)
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="q"></param>
        /// <param name="gain"></param>
        public void Change(double freq, double q = 1, double gain = 1.0)
        {
            SetCoefficients(freq, q, gain);
        }
    }
}