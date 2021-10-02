namespace NWaves.Transforms.Base
{
    /// <summary>
    /// Interface for real-valued transforms.
    /// </summary>
    public interface ITransform
    {
        /// <summary>
        /// Gets transform size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Does direct transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void Direct(float[] input, float[] output);

        /// <summary>
        /// Does normalized direct transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void DirectNorm(float[] input, float[] output);

        /// <summary>
        /// Does inverse transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void Inverse(float[] input, float[] output);

        /// <summary>
        /// Does normalized inverse transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void InverseNorm(float[] input, float[] output);
    }
}
