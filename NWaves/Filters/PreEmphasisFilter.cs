using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Standard pre-emphasis FIR filter
    /// </summary>
    public class PreEmphasisFilter : FirFilter
    {
        /// <summary>
        /// Constructor computes simple 1st order kernel
        /// </summary>
        /// <param name="a">Pre-emphasis coefficient</param>
        public PreEmphasisFilter(double a = 0.97) : base(new [] {1, -a})
        {
        }
    }
}
