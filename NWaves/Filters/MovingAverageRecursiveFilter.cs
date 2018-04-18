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
        /// Delay line
        /// </summary>
        private float _out1;

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

            var b0 = _b32[0];
            var bs = _b32[Size];

            output[0] = input[0] * b0;

            for (var n = 1; n < size; n++)
            {
                output[n] = input[n] * b0 + output[n - 1];
            }

            for (var n = size; n < input.Length; n++)
            {
                output[n] = input[n - size] * bs + input[n] * b0 + output[n - 1];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Online filtering (frame-by-frame)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var output = new float[input.Length];

            var b0 = _b32[0];
            var bs = _b32[Size];

            for (var n = 0; n < input.Length; n++)
            {
                output[n] = b0 * input[n] + bs * _delayLineB[_delayLineOffsetB] + _out1;

                _delayLineB[_delayLineOffsetB] = input[n];
                _out1 = output[n];

                if (--_delayLineOffsetB < 1)
                {
                    _delayLineOffsetB = _delayLineB.Length - 1;
                }
            }

            return output;
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public override void Reset()
        {
            _out1 = 0;
            base.Reset();
        }
    }
}