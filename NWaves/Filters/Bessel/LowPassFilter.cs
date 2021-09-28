using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Bessel
{
    /// <summary>
    /// Represents low-pass Bessel filter.
    /// </summary>
    public class LowPassFilter : ZiFilter
    {
        /// <summary>
        /// Constructs <see cref="LowPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="freq"/>.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        public LowPassFilter(double freq, int order) : base(MakeTf(freq, order))
        {
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        private static TransferFunction MakeTf(double freq, int order)
        {
            return DesignFilter.IirLpTf(freq, PrototypeBessel.Poles(order));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        public void Change(double freq) => Change(MakeTf(freq, _a.Length - 1));
    }
}
