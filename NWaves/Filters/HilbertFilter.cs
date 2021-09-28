using NWaves.Filters.Base;
using System;
using System.Collections.Generic;

namespace NWaves.Filters
{
    /// <summary>
    /// Represents Hilbert filter.
    /// </summary>
    public class HilbertFilter : FirFilter
    {
        /// <summary>
        /// Gets size of the filter.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Constructs <see cref="HilbertFilter"/> of given <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Size of the filter</param>
        public HilbertFilter(int size = 128) : base(MakeKernel(size))
        {
            Size = size;
        }

        /// <summary>
        /// Generates filter kernel of given <paramref name="size"/>.
        /// </summary>
        /// <param name="size">Kernel size</param>
        private static IEnumerable<double> MakeKernel(int size)
        {
            var kernel = new double[size];

            kernel[0] = 0;
            for (var i = 1; i < size; i++)
            {
                kernel[i] = 2 * Math.Pow(Math.Sin(Math.PI * i / 2), 2) / (Math.PI * i);
            }

            return kernel;
        }
    }
}
