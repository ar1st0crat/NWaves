namespace NWaves.Transforms.Base
{
    /// <summary>
    /// Interface for complex-valued transforms.
    /// </summary>
    public interface IComplexTransform
    {
        /// <summary>
        /// Gets transform size.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Direct transform.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        void Direct(float[] inRe, float[] inIm, float[] outRe, float[] outIm);

        /// <summary>
        /// Direct normalized transform.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        void DirectNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm);

        /// <summary>
        /// Inverse transform.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        void Inverse(float[] inRe, float[] inIm, float[] outRe, float[] outIm);

        /// <summary>
        /// Inverse normalized transform.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        void InverseNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm);
    }
}
