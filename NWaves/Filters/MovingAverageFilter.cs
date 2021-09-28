using System.Collections.Generic;
using System.Linq;
using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Provides non-recursive implementation of moving-average filter.
    /// </summary>
    public class MovingAverageFilter : FirFilter
    {
        /// <summary>
        /// Gets size of the filter.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Constructs <see cref="MovingAverageFilter"/> of given <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Size of the filter</param>
        public MovingAverageFilter(int size = 9) : base(MakeKernel(size))
        {
            Size = size;
        }

        /// <summary>
        /// Generates filter kernel of given <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Kernel size</param>
        private static IEnumerable<float> MakeKernel(int size)
        {
            return Enumerable.Repeat(1f / size, size);
        }
    }
}
