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
        public MovingAverageRecursiveFilter(int size = 9) : base(MakeTf(size))
        {
            Size = size;
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static TransferFunction MakeTf(int size)
        {
            if (size % 2 == 0)
            {
                throw new ArgumentException("Size of the filter must be an odd number!");
            }

            var b = Enumerable.Repeat(0.0, size + 1).ToArray();
            b[0] = 1.0 / size;
            b[size] = -1.0 / size;

            var a = new[] { 1, -1.0 };

            return new TransferFunction(b, a);
        }

        /// <summary>
        /// Apply filter by fast recursive strategy
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
            output[0] = input[0] * _b32[0];

            for (var n = 1; n < input.Length; n++)
            {
                if (n >= size) output[n] = input[n - size] * _b32[size];
                output[n] += input[n] * _b32[0] + output[n - 1];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}