using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Bessel
{
    /// <summary>
    /// Represents band-stop Bessel filter.
    /// </summary>
    public class BandStopFilter : ZiFilter
    {
        /// <summary>
        /// Constructs <see cref="BandStopFilter"/> of given <paramref name="order"/> 
        /// with given cutoff frequencies <paramref name="f1"/> and <paramref name="f2"/>.
        /// </summary>
        /// <param name="f1">First cutoff frequency</param>
        /// <param name="f2">Second cutoff frequency</param>
        /// <param name="order">Filter order</param>
        public BandStopFilter(double f1, double f2, int order) : base(MakeTf(f1, f2, order))
        {
        }

        /// <summary>
        /// Generates transfer function.
        /// </summary>
        /// <param name="f1">First cutoff frequency</param>
        /// <param name="f2">Second cutoff frequency</param>
        /// <param name="order">Filter order</param>
        private static TransferFunction MakeTf(double f1, double f2, int order)
        {
            return DesignFilter.IirBsTf(f1, f2, PrototypeBessel.Poles(order));
        }

        /// <summary>
        /// Changes filter coefficients online (preserving the state of the filter).
        /// </summary>
        /// <param name="f1">First cutoff frequency</param>
        /// <param name="f2">Second cutoff frequency</param>
        public void Change(double f1, double f2) => Change(MakeTf(f1, f2, (_a.Length - 1) / 2));
    }
}
