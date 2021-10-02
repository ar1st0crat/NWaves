using NWaves.Transforms.Base;
using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Represents Fast Hartley Transform.
    /// </summary>
    public class HartleyTransform : ITransform
    {
        /// <summary>
        /// Gets size of Hartley transform.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Internal array for imaginary parts.
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Constructs Hartley transformer. Transform <paramref name="size"/> must be a power of 2.
        /// </summary>
        /// <param name="size">Size of Hartley transform</param>
        public HartleyTransform(int size)
        {
            Size = size;
            _fft = new Fft(size);
            _im = new float[size];
        }

        /// <summary>
        /// Does Fast Hartley Transform in-place.
        /// </summary>
        /// <param name="re">Input/output data</param>
        public void Direct(float[] re)
        {
            Array.Clear(_im, 0, _im.Length);

            _fft.Direct(re, _im);

            for (var i = 0; i < re.Length; i++)
            {
                re[i] -= _im[i];
            }
        }

        /// <summary>
        /// Does inverse Hartley transform in-place.
        /// </summary>
        /// <param name="re">Input/output data</param>
        public void Inverse(float[] re) => Direct(re);

        /// <summary>
        /// Does normalized Inverse Fast Hartley transform in-place.
        /// </summary>
        /// <param name="re">Input/output data</param>
        public void InverseNorm(float[] re)
        {
            Direct(re);

            for (var i = 0; i < re.Length; i++)
            {
                re[i] /= Size;
            }
        }

        /// <summary>
        /// Does Fast Hartley Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(float[] input, float[] output)
        {
            input.FastCopyTo(output, input.Length);
            Direct(output);
        }

        /// <summary>
        /// Does normalized Fast Hartley Transform. 
        /// Identical to <see cref="Direct(float[], float[])"/>.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void DirectNorm(float[] input, float[] output)
        {
            input.FastCopyTo(output, input.Length);
            Direct(output);
        }

        /// <summary>
        /// Does Inverse Fast Hartley Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output)
        {
            input.FastCopyTo(output, input.Length);
            Inverse(output);
        }

        /// <summary>
        /// Does normalized Inverse Fast Hartley Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void InverseNorm(float[] input, float[] output)
        {
            input.FastCopyTo(output, input.Length);
            InverseNorm(output);
        }
    }
}
