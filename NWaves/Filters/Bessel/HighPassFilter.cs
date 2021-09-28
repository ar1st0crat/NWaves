using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Bessel
{
    /// <summary>
    /// Represents high-pass Bessel filter.
    /// </summary>
    public class HighPassFilter : ZiFilter
    {
        /// <summary>
        /// Constructs <see cref="HighPassFilter"/> of given <paramref name="order"/> with given cutoff <paramref name="freq"/>.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        public HighPassFilter(double freq, int order) : base(MakeTf(freq, order))
        {
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        /// <param name="order">Filter order</param>
        private static TransferFunction MakeTf(double freq, int order)
        {
            return DesignFilter.IirHpTf(freq, PrototypeBessel.Poles(order));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="freq">Cutoff frequency</param>
        public void Change(double freq) => Change(MakeTf(freq, _a.Length - 1));
    }
}
