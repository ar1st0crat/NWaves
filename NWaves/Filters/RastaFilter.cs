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
        public RastaFilter(double pole = 0.98) : base(new[] { 0.2f, 0.1f, 0, -0.1f, -0.2f }, new[] { 1, -(float)pole })
        {
        }
    }
}
