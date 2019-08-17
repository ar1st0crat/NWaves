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
        /// <param name="size">size of the filter</param>
        public MovingAverageRecursiveFilter(int size = 9) : base(MakeNumerator(size), new[] { 1f, -1 })
        {
            Size = size;
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private static float[] MakeNumerator(int size)
        {
            var b = new float[size + 1];

            b[0] = 1f / size;
            b[size] = -b[0];

            return b;
        }

        /// <summary>
        /// Apply filter by fast recursive strategy
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            if (method != FilteringMethod.Auto)
            {
                return base.ApplyTo(signal, method);
            }

            var input = signal.Samples;
            var size = Size;

            var output = new float[input.Length];

            var b0 = _b[0];
            var bs = _b[Size];

            output[0] = input[0] * b0;

            for (int n = 1, k = 0; n < size; n++, k++)
            {
                output[n] = input[n] * b0 + output[k];
            }

            for (int n = size, k = size - 1, delay = 0; n < input.Length; n++, k++, delay++)
            {
                output[n] = input[delay] * bs + input[n] * b0 + output[k];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var b0 = _b[0];
            var bs = _b[Size];

            var output = b0 * sample + bs * _delayLineB[_delayLineOffsetB] + _out1;

            _delayLineB[_delayLineOffsetB] = sample;
            _out1 = output;

            if (--_delayLineOffsetB < 1)
            {
                _delayLineOffsetB = _numeratorSize - 1;
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
