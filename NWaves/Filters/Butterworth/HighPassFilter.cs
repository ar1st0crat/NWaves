using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Butterworth
{
    /// <summary>
    /// Represents highpass Butterworth filter.
    /// </summary>
    public class HighPassFilter : ZiFilter
    {
        /// <summary>
        /// Gets cutoff frequency.
        /// </summary>
        public double Frequency { get; private set; }

        /// <summary>
        /// Gets filter order.
        /// </summary>
        public int Order => _a.Length - 1;

        /// <summary>
        /// Constructs <see cref="HighPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="frequency"/>.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        public HighPassFilter(double frequency, int order) : base(MakeTf(frequency, order))
        {
            Frequency = frequency;
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        /// <param name="order">Filter order</param>
        private static TransferFunction MakeTf(double frequency, int order)
        {
            return DesignFilter.IirHpTf(frequency, PrototypeButterworth.Poles(order));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="frequency">Normalized cutoff frequency in range [0..0.5]</param>
        public void Change(double frequency)
        {
            Frequency = frequency;

            Change(MakeTf(frequency, _a.Length - 1));
        }
    }
}
