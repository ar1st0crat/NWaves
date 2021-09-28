using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Represents RASTA filter (used for robust speech processing).
    /// </summary>
    public class RastaFilter : IirFilter
    {
        /// <summary>
        /// Constructs <see cref="RastaFilter"/>.
        /// </summary>
        /// <param name="pole">Pole</param>
        public RastaFilter(double pole = 0.98) : base(new[] { 0.2f, 0.1f, 0, -0.1f, -0.2f }, new[] { 1, -(float)pole })
        {
        }
    }
}
