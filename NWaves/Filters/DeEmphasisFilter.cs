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
        public DeEmphasisFilter(float a = 0.97f)
        {
            B = new[] { 1.0f };
            A = new[] { 1, -a };
        }
    }
}
