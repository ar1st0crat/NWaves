using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Elliptic
{
    /// <summary>
    /// Represents low-pass elliptic filter.
    /// </summary>
    public class LowPassFilter : ZiFilter
    {
        /// <summary>
        /// Constructs <see cref="LowPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="freq"/>.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public LowPassFilter(double freq, int order, double ripplePass = 1, double rippleStop = 20) : 
            base(MakeTf(freq, order, ripplePass, rippleStop))
        {
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        private static TransferFunction MakeTf(double freq, int order, double ripplePass = 1, double rippleStop = 20)
        {
            return DesignFilter.IirLpTf(freq,
                                        PrototypeElliptic.Poles(order, ripplePass, rippleStop),
                                        PrototypeElliptic.Zeros(order, ripplePass, rippleStop));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public void Change(double freq, double ripplePass = 1, double rippleStop = 20)
        {
            Change(MakeTf(freq, _a.Length - 1, ripplePass, rippleStop));
        }
    }
}
