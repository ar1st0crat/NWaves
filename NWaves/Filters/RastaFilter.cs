using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// RASTA filter (used for robust speech processing)
    /// </summary>
    public class RastaFilter : IirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RastaFilter() : base(new[] {0.2, 0.1, 0, -0.1, -0.2}, new [] {1, -0.98})
        {
        }
    }
}
