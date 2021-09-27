namespace NWaves.Filters.Base
{
    /// <summary>
    /// State-space representation of filter.
    /// </summary>
    public class StateSpace
    {
        /// <summary>
        /// Gets or sets state matrix.
        /// </summary>
        public double[][] A { get; set; }

        /// <summary>
        /// Gets or sets input-to-state matrix.
        /// </summary>
        public double[] B { get; set; }

        /// <summary>
        /// Gets or sets state-to-output matrix.
        /// </summary>
        public double[] C { get; set; }

        /// <summary>
        /// Gets or sets feedthrough matrix.
        /// </summary>
        public double[] D { get; set; }
    }
}
