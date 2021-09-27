using NWaves.Filters.OnePole;

namespace NWaves.Filters
{
    /// <summary>
    /// Class representing de-emphasis IIR filter.
    /// </summary>
    public class DeEmphasisFilter : OnePoleFilter
    {
        /// <summary>
        /// Construct <see cref="DeEmphasisFilter"/>.
        /// </summary>
        /// <param name="a">De-emphasis coefficient</param>
        /// <param name="normalize">Normalize freq response to unit gain</param>
        public DeEmphasisFilter(double a = 0.97, bool normalize = false) : base(1.0, -a)
        {
            if (normalize)
            {
                _b[0] = (float)(1 - a);
            }
        }
    }
}
