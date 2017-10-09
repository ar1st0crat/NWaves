using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Actually MA filter belongs to FIR filters (so it's inherited from FirFilter);
    /// however it can be realized also (and more efficiently) as recursive filter.
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

        /// <summary>
        /// Actually MA filter belongs to FIR filters (so it's inherited from FirFilter);
        /// however it can be realized also (and more efficiently) as recursive filter.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringOptions filteringOptions = FilteringOptions.OverlapAdd)
        {
            switch (filteringOptions)
            {
                case FilteringOptions.Custom:
                case FilteringOptions.DifferenceEquation:
                    return signal.Copy();
                default:
                    return base.ApplyTo(signal, filteringOptions);
            }
        }
    }
}
