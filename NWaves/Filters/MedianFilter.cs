using System;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters
{
    /// <summary>
    /// Nonlinear median filter
    /// </summary>
    public class MedianFilter : IFilter
    {
        /// <summary>
        /// The size of median filter
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size"></param>
        public MedianFilter(int size = 9)
        {
            if (size % 2 == 0)
            {
                throw new ArgumentException("Size of the filter must be an odd number!");
            }

            Size = size;
        }

        /// <summary>
        /// Method implements median filtering algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            var mid = Size / 2;

            var buf = new float[Size + mid];
            var zeros = new float[Size];

            var n = 0;
            var i = Size / 2;
            while (i >= 0)
            {
                zeros.FastCopyTo(buf, zeros.Length);
                input.FastCopyTo(buf, Size, 0, i--);
                output[n++] = MathUtils.FindNth(buf, mid, 0, Size - 1);
            }

            i = 1;
            while (i <= input.Length - Size)
            {
                input.FastCopyTo(buf, Size, i++);
                output[n++] = MathUtils.FindNth(buf, mid, 0, Size - 1);
            }

            var offset = 1;
            while (i < input.Length - Size / 2)
            {
                zeros.FastCopyTo(buf, zeros.Length, 0, offset);
                input.FastCopyTo(buf, Size - offset, i++, offset);
                output[n++] = MathUtils.FindNth(buf, mid + offset, offset, offset + Size - 1);
                offset++;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
        
        /// <summary>
        /// Online processing (buffer-by-buffer)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            return ApplyTo(new DiscreteSignal(1, input)).Samples;
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public void Reset()
        {
        }
    }
}
