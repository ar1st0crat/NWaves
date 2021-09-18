using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class representing Fast Mellin Transform.
    /// </summary>
    public class MellinTransform
    {
        /// <summary>
        /// Gets the input size of Mellin transform.
        /// </summary>
        public int InputSize { get; private set; }

        /// <summary>
        /// Gets the output size of Mellin transform.
        /// </summary>
        public int OutputSize { get; private set; }

        /// <summary>
        /// Time points on linear scale.
        /// </summary>
        private readonly float[] _linScale;

        /// <summary>
        /// Time points on exponential scale.
        /// </summary>
        private readonly float[] _expScale;

        /// <summary>
        /// FFT transformer.
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Constructs Mellin transformer. 
        /// Parameter <paramref name="outputSize"/> must be a power of 2.
        /// </summary>
        /// <param name="inputSize">Expected size of input data</param>
        /// <param name="outputSize">Size of output data</param>
        public MellinTransform(int inputSize, int outputSize)
        {
            Guard.AgainstNotPowerOfTwo(outputSize, "Output size");

            InputSize = inputSize;
            OutputSize = outputSize;

            _fft = new Fft(outputSize);

            _linScale = Enumerable.Range(0, inputSize)
                                  .Select(i => (float)i / inputSize)
                                  .ToArray();

            _expScale = new float[outputSize];

            var cur = -(float)Math.Log(outputSize);
            var step = -cur / outputSize;

            for (var i = 0; i < _expScale.Length; i++, cur += step)
            {
                _expScale[i] = (float)Math.Exp(cur);
            }
        }

        /// <summary>
        /// Do Fast Mellin Transform.
        /// </summary>
        /// <param name="input">Input array of samples</param>
        /// <param name="outputRe">Output array of real parts</param>
        /// <param name="outputIm">Output array of imaginary parts</param>
        /// <param name="beta">Beta coefficient (0.5 by default, which corresponds to Scale transform)</param>
        /// <param name="normalize">Normalize output by square root of FFT size</param>
        public void Direct(float[] input, float[] outputRe, float[] outputIm, double beta = 0.5, bool normalize = true)
        {
            MathUtils.InterpolateLinear(_linScale, input, _expScale, outputRe);

            for (var i = 0; i < outputRe.Length; i++)
            {
                outputRe[i] *= (float)Math.Pow(_expScale[i], beta);
                outputIm[i] = 0;
            }

            _fft.Direct(outputRe, outputIm);

            if (!normalize)
            {
                return;
            }

            var norm = (float)(1 / Math.Sqrt(outputRe.Length));

            for (var i = 0; i < outputRe.Length; i++)
            {
                outputRe[i] *= norm;
                outputIm[i] *= norm;
            }
        }
    }
}
