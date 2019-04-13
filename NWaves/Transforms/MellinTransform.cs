using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Transforms
{
    public class MellinTransform
    {
        public int InputSize { get; private set; }
        public int OutputSize { get; private set; }

        private readonly float[] _linScale;

        private readonly float[] _expScale;

        private readonly Fft _fft;

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
        /// Direct Fast Mellin Transform
        /// </summary>
        /// <param name="input"></param>
        /// <param name="outputRe"></param>
        /// <param name="outputIm"></param>
        /// <param name="normalize"></param>
        public void Direct(float[] input, float[] outputRe, float[] outputIm, bool normalize = true)
        {
            MathUtils.InterpolateLinear(_linScale, input, _expScale, outputRe);

            for (var i = 0; i < outputRe.Length; i++)
            {
                outputRe[i] *= (float)Math.Pow(_expScale[i], 0.5);
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
