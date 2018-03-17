using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// DC removal IIR filter
    /// </summary>
    public class DcRemovalFilter : IirFilter
    {
        /// <summary>
        /// Constructor creates simple 1st order recursive filter
        /// </summary>
        /// <param name="r">R coefficient (usually in [0.9, 1] range)</param>
        public DcRemovalFilter(float r = 0.995f) : base(new [] {1, -1.0f}, new [] {1, -r})
        {
        }
    }
}
