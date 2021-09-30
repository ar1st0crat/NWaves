using NWaves.Signals;
using NWaves.Transforms.Base;
using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Represents Fast Hilbert Transform.
    /// </summary>
    public class HilbertTransform : ITransform
    {
        /// <summary>
        /// Gets size of Hilbert transform.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Intermediate buffer for real parts.
        /// </summary>
        private readonly float[] _re;

        /// <summary>
        /// Intermediate buffer for imaginary parts.
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Constructs Hilbert transformer. Transform <paramref name="size"/> must be a power of 2.
        /// </summary>
        public HilbertTransform(int size = 512)
        {
            Size = size;
            _fft = new Fft(size);
            _re = new float[size];
            _im = new float[size];
        }

        /// <summary>
        /// Computes complex analytic signal (real and imaginary parts) from <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Input data</param>
        public ComplexDiscreteSignal AnalyticSignal(float[] input)
        {
            Direct(input, _im);

            for (int i = 0; i < Size; i++)
            {
                _re[i] /= Size;
                _im[i] /= Size;
            }

            return new ComplexDiscreteSignal(1, _re.ToDoubles(), _im.ToDoubles(), allocateNew: true);
        }

        /// <summary>
        /// Does Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(float[] input, float[] output)
        {
            // just here, for code brevity, use alias _im for output (i.e. it's not internal _im)
            var _im = output;

            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);

            input.FastCopyTo(_re, input.Length);

            _fft.Direct(_re, _im);

            for (var i = 1; i < _re.Length / 2; i++)
            {
                _re[i] *= 2;
                _im[i] *= 2;
            }

            for (var i = _re.Length / 2 + 1; i < _re.Length; i++)
            {
                _re[i] = 0.0f;
                _im[i] = 0.0f;
            }

            _fft.Inverse(_re, _im);
        }

        /// <summary>
        /// Does normalized Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void DirectNorm(float[] input, float[] output)
        {
            Direct(input, output);

            for (int i = 0; i < Size; i++)
            {
                output[i] /= Size;
            }
        }

        /// <summary>
        /// Does Inverse Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Inverse(float[] input, float[] output)
        {
            Direct(input, output);

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = -output[i];
            }
        }

        /// <summary>
        /// Does normalized Inverse Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void InverseNorm(float[] input, float[] output)
        {
            DirectNorm(input, output);

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = -output[i];
            }
        }
    }
}
