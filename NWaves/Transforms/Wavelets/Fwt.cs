using NWaves.Utils;
using System;
using System.Linq;

namespace NWaves.Transforms.Wavelets
{
    /// <summary>
    /// Fast Wavelet Transform
    /// </summary>
    public class Fwt
    {
        /// <summary>
        /// Size of the transform
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// The length of the mother wavelet
        /// </summary>
        protected int _waveletLength;

        /// <summary>
        /// LP coefficients for decomposition
        /// </summary>
        protected float[] _loD;

        /// <summary>
        /// HP coefficients for decomposition
        /// </summary>
        protected float[] _hiD;

        /// <summary>
        /// LP coefficients for reconstruction
        /// </summary>
        protected float[] _loR;

        /// <summary>
        /// HP coefficients for reconstruction
        /// </summary>
        protected float[] _hiR;

        /// <summary>
        /// Temporary internal buffer
        /// </summary>
        protected float[] _temp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"></param>
        /// <param name="wavelet"></param>
        public Fwt(int size, Wavelet wavelet)
        {
            Size = size;

            _waveletLength = wavelet.Length;

            _loD = wavelet.LoD.Reverse().ToArray(); // reverse due to a specific processing later
            _hiD = wavelet.HiD.Reverse().ToArray();
            _loR = wavelet.LoR.ToArray();           // in orthonormal case: loR = loD and hiR = hiD
            _hiR = wavelet.HiR.ToArray();
            
            _temp = new float[size];

            // For future:
            // - reserve memory for all modes of signal extension
            //   (so far only 'periodization' mode is coded)

            //var halflen = (int)((size + _waveletLength - 1) * 0.5); // convolution length: N + M - 1
            //_temp = new float[halflen * 2];
        }

        /// <summary>
        /// Direct FWT (decomposition)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="level"></param>
        public void Direct(float[] input, float[] output, int level = 0)
        {
            var maxLevel = MaxLevel(input.Length);

            if (level <= 0)
            {
                level = maxLevel;
            }
            else if (level > maxLevel)
            {
                throw new ArgumentException($"Specified level is too large for input array. Max level is {maxLevel}");
            }

            input.FastCopyTo(_temp, input.Length);

            var h = input.Length;

            for (var l = 0; l < level && h >= _waveletLength; l++, h /= 2)
            {
                var halfLen = h / 2;

                var start = _waveletLength / 4;

                for (int i = 0; i < halfLen; i++, start++)
                {
                    if (start == halfLen) start = 0;

                    output[start] = output[start + halfLen] = 0;

                    for (int j = 0; j < _waveletLength; j++)
                    {
                        var k = (i * 2 + j) % h;

                        output[start]           += _temp[k] * _loD[j]; // approximation
                        output[start + halfLen] += _temp[k] * _hiD[j]; // details
                    }
                }

                output.FastCopyTo(_temp, h);
            }
        }

        /// <summary>
        /// Inverse FWT (reconstruction)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="level"></param>
        public void Inverse(float[] input, float[] output, int level = 0)
        {
            var maxLevel = MaxLevel(input.Length);

            if (level <= 0)
            {
                level = maxLevel;
            }
            else if (level > maxLevel)
            {
                throw new ArgumentException($"Specified level is too large for input array. Max level is {maxLevel}");
            }

            input.FastCopyTo(_temp, input.Length);

            var h = (int)(input.Length / Math.Pow(2, level - 1));

            for (; h <= input.Length; h *= 2)
            {
                Array.Clear(output, 0, output.Length);

                var halfLen = h / 2;

                var start = _waveletLength / 4;

                for (int i = 0; i < halfLen; i++, start++)
                {
                    if (start == halfLen) start = 0;

                    for (int j = 0; j < _waveletLength; j++)
                    {
                        var k = (i * 2 + j) % h;

                        output[k] += _temp[start] * _loR[j] + _temp[start + halfLen] * _hiR[j];
                    }
                }

                output.FastCopyTo(_temp, h);
            }
        }

        /// <summary>
        /// Maximum decomposition level
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public int MaxLevel(int length) => (int)(Math.Log(length / (_waveletLength - 1), 2));
    }
}
