using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Standard de-emphasis IIR filter
    /// </summary>
    public class DeEmphasisFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a">De-emphasis coefficient</param>
        public DeEmphasisFilter(double a = 0.97)
        {
            B = new[] { 1.0 };
            A = new[] { 1, -a };
        }
    }
}
