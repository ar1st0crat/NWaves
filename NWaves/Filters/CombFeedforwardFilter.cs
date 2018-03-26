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
        public CombFeedforwardFilter(int m, double b0 = 1.0, double bm = 1.0) : base(MakeKernel(m, b0, bm))
        {
            _delay = m;
        }

        /// <summary>
        /// Kernel generator
        /// </summary>
        /// <param name="m">Delay</param>
        /// <param name="b0">Coefficient b0</param>
        /// <param name="bm">Coefficient bm</param>
        private static double[] MakeKernel(int m, double b0, double bm)
        {
            var kernel = new double[m + 1];
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
            var output = new float[input.Length];

            input.FastCopyTo(output, _delay);

            var b0 = (float)Kernel[0];
            var bm = (float)Kernel[_delay];

            for (var i = _delay; i < signal.Length; i++)
            {
                output[i] = b0 * input[i] + bm * input[i - _delay];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
