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
        public DeEmphasisFilter(double a = 0.97) : base(new[] { 1.0 }, new[] { 1, -a })
        {
        }
    }
}
