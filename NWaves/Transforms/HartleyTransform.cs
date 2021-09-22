using NWaves.Transforms.Base;
using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class representing Fast Hartley Transform.
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
        /// Construct Hartley transformer. Transform <paramref name="size"/> must be a power of 2.
        /// </summary>
        /// <param name="size">Size of Hartley transform</param>
        public HartleyTransform(int size)
        {
            Size = size;
            _fft = new Fft(size);
            _im = new float[size];
        }

        /// <summary>
        /// Do Fast Hartley Transform in-place.
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
        /// Do inverse Hartley transform in-place.
        /// </summary>
        /// <param name="re">Input/output data</param>
        public void Inverse(float[] re)
        {
            _im[0] = 0;
            
            for (var i = 1; i <= re.Length / 2; i++)
            {
                var x = (re[Size - i] - re[i]) * 0.5f;
                _im[i] = x;
                _im[Size - i] = -x;

                x = (re[i] + re[Size - i]) * 0.5f;
                re[i] = re[Size - i] = x;
            }

            _fft.Inverse(re, _im);
        }

        /// <summary>
        /// Do normalized Inverse Fast Hartley transform in-place.
        /// </summary>
        /// <param name="re">Input/output data</param>
        public void InverseNorm(float[] re)
        {
            _im[0] = 0;

            for (var i = 1; i <= re.Length / 2; i++)
            {
                var x = (re[Size - i] - re[i]) * 0.5f;
                _im[i] = x;
                _im[Size - i] = -x;

                x = (re[i] + re[Size - i]) * 0.5f;
                re[i] = re[Size - i] = x;
            }

            _fft.Inverse(re, _im);

            for (var i = 0; i < re.Length; i++)
            {
                re[i] /= Size;
            }
        }

        /// <summary>
        /// Do Fast Hartley Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(float[] input, float[] output)
        {
            input.FastCopyTo(output, input.Length);
            Direct(output);
        }

        /// <summary>
        /// Do normalized Fast Hartley Transform. 
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
        /// Do Inverse Fast Hartley Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output)
        {
            input.FastCopyTo(output, input.Length);
            Inverse(output);
        }

        /// <summary>
        /// Do normalized Inverse Fast Hartley Transform.
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
