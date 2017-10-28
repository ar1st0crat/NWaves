using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NWaves.Filters.Base;
using NWaves.Signals;

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
        /// 
        /// </summary>
        public override ComplexDiscreteSignal Zeros
        {
            get
            {
                var re = new double[Size];
                var im = new double[Size];
                for (var i = 0; i < Size; i++)
                {
                    re[i] = Math.Cos(2 * Math.PI * i / Size);
                    im[i] = Math.Sin(2 * Math.PI * i / Size);
                }
                return new ComplexDiscreteSignal(1, re, im);
            }
        }
    }

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
            
            var samples = new double[input.Length];
            samples[0] = input[0] / size;

            for (var n = 1; n < input.Length; n++)
            {
                if (n >= size) samples[n] -= input[n - size] / size;
                samples[n] += input[n] / size + samples[n - 1];
            }

            return new DiscreteSignal(signal.SamplingRate, samples);
        }

        /// <summary>
        /// 
        /// </summary>
        public override ComplexDiscreteSignal Zeros
        {
            get
            {
                var re = new double[Size];
                var im = new double[Size];
                for (var i = 0; i < Size; i++)
                {
                    re[i] = Math.Cos(2 * Math.PI * i / Size);
                    im[i] = Math.Sin(2 * Math.PI * i / Size);
                }
                return new ComplexDiscreteSignal(1, re, im);
            }
        }
    }
}
