using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters
{
    /// <summary>
    /// Feedforward comb filter:
    /// 
    ///     y[n] = b0 * x[n] + bm * x[n - m]
    /// 
    /// </summary>
    public class CombFeedforwardFilter : FirFilter
    {
        /// <summary>
        /// Delay (m)
        /// </summary>
        private readonly int _delay;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="m">Delay</param>
        /// <param name="b0">Coefficient b0</param>
        /// <param name="bm">Coefficient bm</param>
        public CombFeedforwardFilter(int m, float b0 = 1.0f, float bm = 1.0f) : base(MakeKernel(m, b0, bm))
        {
            _delay = m;
        }

        /// <summary>
        /// Static helper method
        /// </summary>
        /// <param name="m"></param>
        /// <param name="b0"></param>
        /// <param name="bm"></param>
        /// <returns></returns>
        private static float[] MakeKernel(int m, float b0, float bm)
        {
            var kernel = new float[m + 1];
            kernel[0] = b0;
            kernel[m] = bm;

            return kernel;
        }

        /// <summary>
        /// Apply filter
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
            var output = new float [input.Length];

            FastCopy.ToExistingArray(input, output, _delay);

            var b0 = Kernel[0];
            var bm = Kernel[_delay];

            for (var i = _delay; i < signal.Length; i++)
            {
                output[i] = b0 * input[i] + bm * input[i - _delay];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
