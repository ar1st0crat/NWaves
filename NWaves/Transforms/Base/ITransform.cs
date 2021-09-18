namespace NWaves.Transforms.Base
{
    /// <summary>
    /// Interface for real-valued transforms.
    /// </summary>
    public interface ITransform
    {
        /// <summary>
        /// Transform size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Direct transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void Direct(float[] input, float[] output);

        /// <summary>
        /// Direct normalized transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void DirectNorm(float[] input, float[] output);

        /// <summary>
        /// Inverse transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void Inverse(float[] input, float[] output);

        /// <summary>
        /// Inverse normalized transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        void InverseNorm(float[] input, float[] output);
    }
}
