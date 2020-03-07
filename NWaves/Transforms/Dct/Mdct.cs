using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class providing methods for Modified Discrete Cosine Transform (MDCT).
    /// </summary>
    public class Mdct : IDct
    {
        /// <summary>
        /// Internal DCT-IV transformer
        /// </summary>
        private readonly IDct _dct;
        
        /// <summary>
        /// Internal temporary buffer
        /// </summary>
        private readonly float[] _temp;

        /// <summary>
        /// MDCT size
        /// </summary>
        public int Size => _dct.Size;


        public Mdct(int dctSize, IDct dct = null)
        {
            _dct = dct ?? new Dct4(dctSize);
            _temp = new float[dctSize];
        }

        /// <summary>
        /// Direct MDCT
        /// </summary>
        /// <param name="input">Input length must be equal to 2*DctSize</param>
        /// <param name="output">Output length must be equal to DctSize</param>
        public void Direct(float[] input, float[] output)
        {
            int N = _dct.Size;

            for (int n = 0; n < N / 2; n++)
            {
                _temp[n] = -input[3 * N / 2 - 1 - n] - input[3 * N / 2 + n];
            }
            for (int n = N / 2; n < N; n++)
            {
                _temp[n] = input[n - N / 2] - input[3 * N / 2 - 1 - n];
            }
            
            _dct.Direct(_temp, output);
        }

        /// <summary>
        /// Inverse MDCT
        /// </summary>
        /// <param name="input">Input length must be equal to DctSize</param>
        /// <param name="output">Output length must be equal to 2*DctSize</param>
        public void Inverse(float[] input, float[] output)
        {
            int N = _dct.Size;

            _dct.Direct(input, _temp);

            for (int i = N, k = N / 2 - 1; i < 3 * N / 2 && k >= 0; i++, k--)
            {
                output[i] = -_temp[k];
            }
            for (int i = 3 * N / 2, k = 0; i < 2 * N && k < N / 2; i++, k++)
            {
                output[i] = -_temp[k];
            }
            for (int i = 0, k = N / 2; i < N / 2 && k < N; i++, k++)
            {
                output[i] = _temp[k];
            }
            for (int i = N / 2, k = N / 2 - 1; i < N && k >= 0; i++, k--)
            {
                output[i] = -output[k];
            }
        }

        /// <summary>
        /// Direct MDCT (with normalization)
        /// </summary>
        /// <param name="input">Input length must be equal to 2*DctSize</param>
        /// <param name="output">Output length must be equal to DctSize</param>
        public void DirectNorm(float[] input, float[] output)
        {
            Direct(input, output);

            var norm = 2 * (float)Math.Sqrt(2 * _dct.Size);
            for (var i = 0; i < output.Length; output[i++] /= norm) ;
        }

        /// <summary>
        /// Inverse MDCT (with normalization)
        /// </summary>
        /// <param name="input">Input length must be equal to DctSize</param>
        /// <param name="output">Output length must be equal to 2*DctSize</param>
        public void InverseNorm(float[] input, float[] output)
        {
            Inverse(input, output);

            var norm = (float)Math.Sqrt(2 * _dct.Size);
            for (int i = 0; i < output.Length; output[i++] /= norm) ;
        }
    }
}
