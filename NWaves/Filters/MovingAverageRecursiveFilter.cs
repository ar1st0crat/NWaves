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
            
            B = Enumerable.Repeat(0.0, size + 1).ToArray();
            B[0] = 1.0 / size;
            B[size] = -1.0 / size;

            A = new[] { 1, -1.0 };
        }

        /// <summary>
        /// Apply filter by fast recursive strategy.
        /// 
        /// Note. 
        /// Working with double coefficients, since floats lead to crucial lost of precision.
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            if (filteringOptions != FilteringOptions.Auto)
            {
                return base.ApplyTo(signal, filteringOptions);
            }

            var input = signal.Samples;
            var size = Size;

            var output = new float[input.Length];
            output[0] = (float)(input[0] * _b[0]);

            for (var n = 1; n < input.Length; n++)
            {
                if (n >= size) output[n] = (float)(input[n - size] * _b[size]);
                output[n] = (float)(output[n] + input[n] * _b[0] + output[n - 1]);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}