using System;
using System.Linq;
using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Actually MA filter belongs to FIR filters (so it's inherited from FirFilter);
    /// however it can be realized also (and more efficiently) as a recursive filter.
    /// </summary>
    public class MovingAverageFilter : FirFilter
    {
        /// <summary>
        /// Size of the filter: number of samples for averaging
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="size">size of the filter (must be odd number)</param>
        public MovingAverageFilter(int size = 9)
        {
            if (size % 2 == 0)
            {
                throw new ArgumentException("Size of the filter must be an odd number!");
            }

            Size = size;
            Kernel = Enumerable.Repeat(1.0 / size, size).ToArray();
        }
    }

    /// <summary>
    /// Recursive implementation of N-sample MA filter:
    /// 
    ///     y[n] = x[n] / N + x[n - N] / N + y[n - 1]
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
            A = new [] { 1, -1.0 };
            B = Enumerable.Repeat(0.0, size + 1).ToArray();
            B[0] = 1.0 / size;
            B[size] = -1.0 / size;
        }
    }
}
