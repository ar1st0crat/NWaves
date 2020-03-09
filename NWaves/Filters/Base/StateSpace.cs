namespace NWaves.Filters.Base
{
    public class StateSpace
    {
        /// <summary>
        /// State matrix
        /// </summary>
        public double[][] A { get; set; }

        /// <summary>
        /// Input-to-state matrix
        /// </summary>
        public double[] B { get; set; }

        /// <summary>
        /// State-to-output matrix
        /// </summary>
        public double[] C { get; set; }

        /// <summary>
        /// Feedthrough matrix
        /// </summary>
        public double[] D { get; set; }
    }
}
