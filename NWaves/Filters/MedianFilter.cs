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
            Size = size;
        }

        /// <summary>
        /// Method implements median filtering algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Custom)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            var mid = (Size - 1) / 2;

            var buf = input.FastCopyFragment(Size);
            var value = FindNth(buf, mid, 0, Size - 1);
            
            for (var i = 0; i < Size; i++)
            {
                output[i] = value;
            }

            var n = Size;
            for (var i = 1; i < input.Length - Size + 1; i++)
            {
                input.FastCopyTo(buf, Size, i);
                output[n++] = FindNth(buf, mid, 0, Size - 1);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static int Partition(float[] a, int start, int end)
        {
            var pivot = a[end];
            var last = start - 1;
            for (var i = start; i < end; i++)
            {
                if (a[i] <= pivot)
                {
                    last++;
                    var temp = a[i]; a[i] = a[last]; a[last] = temp;
                }
            }
            last++;
            var tmp = a[end]; a[end] = a[last]; a[last] = tmp;

            return last;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static float FindNth(float[] a, int n, int start, int end)
        {
            while (true)
            {
                var pivot = Partition(a, start, end);

                if (pivot == n)
                {
                    return a[pivot];
                }
                if (n < pivot)
                {
                    end = pivot - 1;
                }
                else
                {
                    start = pivot + 1;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public float[] Process(float[] input)
        {
            throw new NotImplementedException();
        }
    }
}
