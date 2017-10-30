using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class ButterworthFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="order"></param>
        public ButterworthFilter(double freq, int order)
        {
            // K=1,2,...,n
            // poles[K] = freq * [-sin(pi * (2K-1) / 2n) + j cos(pi * (2K-1) / 2n)]
        }
    }
}
