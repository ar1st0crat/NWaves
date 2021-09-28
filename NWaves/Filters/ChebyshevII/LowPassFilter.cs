using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.ChebyshevII
{
    /// <summary>
    /// Represents low-pass Chebyshev-II filter.
    /// </summary>
    public class LowPassFilter : ZiFilter
    {
        /// <summary>
        /// Constructs <see cref="LowPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="freq"/>.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripple">Ripple (in dB)</param>
        public LowPassFilter(double freq, int order, double ripple = 0.1) : base(MakeTf(freq, order, ripple))
        {
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripple">Ripple (in dB)</param>
        private static TransferFunction MakeTf(double freq, int order, double ripple = 0.1)
        {
            return DesignFilter.IirLpTf(freq,
                                        PrototypeChebyshevII.Poles(order, ripple),
                                        PrototypeChebyshevII.Zeros(order));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="ripple">Ripple (in dB)</param>
        public void Change(double freq, double ripple = 0.1) => Change(MakeTf(freq, _a.Length - 1, ripple));
    }
}
