using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Elliptic
{
    /// <summary>
    /// Represents band-pass elliptic filter.
    /// </summary>
    public class BandPassFilter : ZiFilter
    {
        /// <summary>
        /// Constructs <see cref="BandPassFilter"/> of given <paramref name="order"/> 
        /// with given cutoff frequencies <paramref name="freq1"/> and <paramref name="freq2"/>.
        /// </summary>
        /// <param name="freq1">First cutoff frequency</param>
        /// <param name="freq2">Second cutoff frequency</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public BandPassFilter(double freq1, double freq2, int order, double ripplePass = 1, double rippleStop = 20) :
            base(MakeTf(freq1, freq2, order, ripplePass, rippleStop))
        {
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="freq1">First cutoff frequency</param>
        /// <param name="freq2">Second cutoff frequency</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        private static TransferFunction MakeTf(double freq1, double freq2, int order, double ripplePass = 1, double rippleStop = 20)
        {
            return DesignFilter.IirBpTf(freq1, freq2,
                                        PrototypeElliptic.Poles(order, ripplePass, rippleStop),
                                        PrototypeElliptic.Zeros(order, ripplePass, rippleStop));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq1">First cutoff frequency</param>
        /// <param name="freq2">Second cutoff frequency</param>
        /// <param name="ripplePass">Passband ripple (in dB)</param>
        /// <param name="rippleStop">Stopband ripple (in dB)</param>
        public void Change(double freq1, double freq2, double ripplePass = 1, double rippleStop = 20)
        {
            Change(MakeTf(freq1, freq2, (_a.Length - 1) / 2, ripplePass, rippleStop));
        }
    }
}
