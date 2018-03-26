using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Class providing non-recursive implementation of N-sample MA filter.
    /// 
    /// Actually MA filter belongs to FIR filters (so it's inherited from FirFilter);
    /// however it can be realized also (and more efficiently) as a recursive filter (see below).
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
        public MovingAverageFilter(int size = 9) : base(MakeKernel(size))
        {
            Size = size;
        }

        /// <summary>
        /// Kernel generator
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static IEnumerable<double> MakeKernel(int size)
        {
            if (size % 2 == 0)
            {
                throw new ArgumentException("Size of the filter must be an odd number!");
            }

            return Enumerable.Repeat(1.0 / size, size);
        }
    }
}
