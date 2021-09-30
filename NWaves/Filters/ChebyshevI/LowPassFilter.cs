using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.ChebyshevI
{
    /// <summary>
    /// Represents lowpass Chebyshev-I filter.
    /// </summary>
    public class LowPassFilter : ZiFilter
    {
        /// <summary>
        /// Gets cutoff frequency.
        /// </summary>
        public double Frequency { get; private set; }

        /// <summary>
        /// Gets ripple (in dB).
        /// </summary>
        public double Ripple { get; private set; }

        /// <summary>
        /// Gets filter order.
        /// </summary>
        public int Order => _a.Length - 1;

        /// <summary>
        /// Constructs <see cref="LowPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripple">Ripple (in dB)</param>
        public LowPassFilter(double frequency, int order, double ripple = 0.1) : base(MakeTf(frequency, order, ripple))
        {
            Frequency = frequency;
            Ripple = ripple;
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        /// <param name="ripple">Ripple (in dB)</param>
        private static TransferFunction MakeTf(double frequency, int order, double ripple = 0.1)
        {
            return DesignFilter.IirLpTf(frequency, PrototypeChebyshevI.Poles(order, ripple));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="ripple">Ripple (in dB)</param>
        public void Change(double frequency, double ripple = 0.1)
        {
            Frequency = frequency;
            Ripple = ripple;

            Change(MakeTf(frequency, _a.Length - 1, ripple));
        }
    }
}
