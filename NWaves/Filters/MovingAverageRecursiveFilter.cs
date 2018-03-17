using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Class providing recursive implementation of N-sample MA filter:
    /// 
    ///     y[n] = x[n] / N - x[n - N] / N + y[n - 1]
    /// 
    /// i.e. 
    ///     B = [1/N, 0, 0, 0, 0, ... , 0, -1/N]
    ///     A = [1, -1]
    /// 
    /// </summary>
    public class MovingAverageRecursiveFilter : IirFilter
    {
        /// <summary>
        /// Size of the filter: number of samples for averaging
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">size of the filter (must be odd number)</param>
        public MovingAverageRecursiveFilter(int size = 9)
        {
            if (size % 2 == 0)
            {
                throw new ArgumentException("Size of the filter must be an odd number!");
            }

            Size = size;
            
            B = Enumerable.Repeat(0.0f, size + 1).ToArray();
            B[0] = 1.0f / size;
            B[size] = -1.0f / size;

            A = new[] { 1, -1.0f };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
            FilteringOptions filteringOptions = FilteringOptions.Custom)
        {
            if (filteringOptions != FilteringOptions.Custom)
            {
                return base.ApplyTo(signal, filteringOptions);
            }

            var input = signal.Samples;
            var size = Size;
            
            var samples = new float[input.Length];
            samples[0] = input[0] / size;

            for (var n = 1; n < input.Length; n++)
            {
                if (n >= size) samples[n] -= input[n - size] / size;
                samples[n] += input[n] / size + samples[n - 1];
            }

            return new DiscreteSignal(signal.SamplingRate, samples);
        }
    }
}