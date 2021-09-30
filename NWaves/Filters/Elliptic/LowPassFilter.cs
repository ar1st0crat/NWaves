using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Elliptic
{
    /// <summary>
    /// Represents lowpass elliptic filter.
    /// </summary>
    public class LowPassFilter : ZiFilter
    {
        /// <summary>
        /// Gets cutoff frequency.
        /// </summary>
        public double Frequency { get; private set; }

        /// <summary>
        /// Gets passband ripple (in dB).
        /// </summary>
        public double RipplePassband { get; private set; }

        /// <summary>
        /// Gets stopband ripple (in dB).
        /// </summary>
        public double RippleStopband { get; private set; }

        /// <summary>
        /// Gets filter order.
        /// </summary>
        public int Order => _a.Length - 1;

        /// <summary>
        /// Constructs <see cref="LowPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public LowPassFilter(double frequency, int order, double ripplePass = 1, double rippleStop = 20) : 
            base(MakeTf(frequency, order, ripplePass, rippleStop))
        {
            Frequency = frequency;
            RipplePassband = ripplePass;
            RippleStopband = rippleStop;
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        private static TransferFunction MakeTf(double frequency, int order, double ripplePass = 1, double rippleStop = 20)
        {
            return DesignFilter.IirLpTf(frequency,
                                        PrototypeElliptic.Poles(order, ripplePass, rippleStop),
                                        PrototypeElliptic.Zeros(order, ripplePass, rippleStop));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public void Change(double frequency, double ripplePass = 1, double rippleStop = 20)
        {
            Frequency = frequency;
            RipplePassband = ripplePass;
            RippleStopband = rippleStop;

            Change(MakeTf(frequency, _a.Length - 1, ripplePass, rippleStop));
        }
    }
}
