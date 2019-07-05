namespace NWaves.Transforms
{
    public interface IDct
    {
        /// <summary>
        /// Direct DCT
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        void Direct(float[] input, float[] output);

        /// <summary>
        /// Direct normalized DCT
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        void DirectNorm(float[] input, float[] output);

        /// <summary>
        /// Inverse DCT
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        void Inverse(float[] input, float[] output);
    }
}