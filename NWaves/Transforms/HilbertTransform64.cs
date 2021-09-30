using NWaves.Signals;
using NWaves.Utils;
using System;

namespace NWaves.Transforms
{
    /// <summary>
    /// Represents Fast Hilbert Transform (for 64-bit data).
    /// </summary>
    public class HilbertTransform64
    {
        /// <summary>
        /// Gets size of Hilbert transform.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly Fft64 _fft;

        /// <summary>
        /// Intermediate buffer for real parts.
        /// </summary>
        private readonly double[] _re;

        /// <summary>
        /// Intermediate buffer for imaginary parts.
        /// </summary>
        private readonly double[] _im;

        /// <summary>
        /// Constructs Hilbert transformer. Transform <paramref name="size"/> must be a power of 2.
        /// </summary>
        public HilbertTransform64(int size = 512)
        {
            Size = size;
            _fft = new Fft64(size);
            _re = new double[size];
            _im = new double[size];
        }

        /// <summary>
        /// Computes complex analytic signal (real and imaginary parts) from <paramref name="input"/>.
        /// </summary>
        /// <param name="input">Input data</param>
        public ComplexDiscreteSignal AnalyticSignal(double[] input)
        {
            Direct(input, _im);

            for (int i = 0; i < Size; i++)
            {
                _re[i] /= Size;
                _im[i] /= Size;
            }

            return new ComplexDiscreteSignal(1, _re, _im, allocateNew: true);
        }

        /// <summary>
        /// Does Fast Hilbert Transform.
        /// </summary>
        /// <param name="input">Input data</param>
        /// <param name="output">Output data</param>
        public void Direct(double[] input, double[] output)
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
        public void DirectNorm(double[] input, double[] output)
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
        public void Inverse(double[] input, double[] output)
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
        public void InverseNorm(double[] input, double[] output)
        {
            DirectNorm(input, output);

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = -output[i];
            }
        }
    }
}
