using NWaves.Filters.Base;
using NWaves.Filters.Fda;

namespace NWaves.Filters.Butterworth
{
    /// <summary>
    /// Class for Butterworth IIR BS filter.
    /// </summary>
    public class BandStopFilter : ZiFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        public BandStopFilter(double f1, double f2, int order) : base(MakeTf(f1, f2, order))
        {
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(double f1, double f2, int order)
        {
            return DesignFilter.IirBsTf(f1, f2, PrototypeButterworth.Poles(order));
        }

        /// <summary>
        /// Change filter coeffs online
        /// </summary>
        ///<param name="f1"></param>
        ///<param name="f2"></param>
        public void Change(double f1, double f2) => Change(MakeTf(f1, f2, (_a.Length - 1) / 2));
    }
}
