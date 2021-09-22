using NWaves.Transforms.Base;
using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Transforms
{
    /// <summary>
    /// Class representing Fast Mellin Transform.
    /// </summary>
    public class MellinTransform : IComplexTransform
    {
        /// <summary>
        /// Gets the size of input data for Mellin transform.
        /// </summary>
        public int InputSize { get; private set; }

        /// <summary>
        /// Gets the size of Mellin transform.
        /// </summary>
        public int Size { get; private set; }

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
        private readonly RealFft _fft;

        /// <summary>
        /// Constructs Mellin transformer. 
        /// Parameter <paramref name="size"/> must be a power of 2.
        /// </summary>
        /// <param name="inputSize">Expected size of input data</param>
        /// <param name="size">Size of output data</param>
        public MellinTransform(int inputSize, int size)
        {
            Guard.AgainstNotPowerOfTwo(size, "Output size of Mellin Transform");

            InputSize = inputSize;
            Size = size;

            _fft = new RealFft(size);

            _linScale = Enumerable.Range(0, inputSize)
                                  .Select(i => (float)i / inputSize)
                                  .ToArray();

            _expScale = new float[size];

            var cur = -(float)Math.Log(size);
            var step = -cur / size;

            for (var i = 0; i < _expScale.Length; i++, cur += step)
            {
                _expScale[i] = (float)Math.Exp(cur);
            }
        }

        /// <summary>
        /// Do Fast Mellin Transform.
        /// </summary>
        /// <param name="input">Input array of samples</param>
        /// <param name="outRe">Output array of real parts</param>
        /// <param name="outIm">Output array of imaginary parts</param>
        /// <param name="beta">Beta coefficient (0.5 by default, which corresponds to Scale transform)</param>
        /// <param name="normalize">Normalize output by square root of FFT size</param>
        public void Direct(float[] input, float[] outRe, float[] outIm, double beta = 0.5, bool normalize = true)
        {
            MathUtils.InterpolateLinear(_linScale, input, _expScale, outRe);

            for (var i = 0; i < outRe.Length; i++)
            {
                outRe[i] *= (float)Math.Pow(_expScale[i], beta);
                outIm[i] = 0;
            }

            _fft.Direct(outRe, outRe, outIm);

            if (!normalize)
            {
                return;
            }

            var norm = (float)(1 / Math.Sqrt(outRe.Length));

            for (var i = 0; i < outRe.Length; i++)
            {
                outRe[i] *= norm;
                outIm[i] *= norm;
            }
        }

        /// <summary>
        /// Do Fast Mellin Transform.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void Direct(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            Direct(inRe, outRe, outIm, normalize: false);
        }

        /// <summary>
        /// Do normalized Fast Mellin Transform.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void DirectNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            Direct(inRe, outRe, outIm, normalize: true);
        }

        /// <summary>
        /// Inverse Fast Mellin Transform is not implemented.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void Inverse(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inverse normalized Fast Mellin Transform is not implemented.
        /// </summary>
        /// <param name="inRe">Input data (real parts)</param>
        /// <param name="inIm">Input data (imaginary parts)</param>
        /// <param name="outRe">Output data (real parts)</param>
        /// <param name="outIm">Output data (imaginary parts)</param>
        public void InverseNorm(float[] inRe, float[] inIm, float[] outRe, float[] outIm)
        {
            throw new NotImplementedException();
        }
    }
}
