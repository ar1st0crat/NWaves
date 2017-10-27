using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// 
    /// </summary>
    public class PreEmphasisFilter : FirFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public PreEmphasisFilter(double a = 0.97)
        {
            Kernel = new [] {1, -a};
        }
    }
}
