using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters
{
    /// <summary>
    /// Feedback comb filter:
    /// 
    ///     y[n] = b0 * x[n] - am * y[n - m]
    /// 
    /// </summary>
    public class CombFeedbackFilter : IirFilter
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
        /// <param name="am">Coefficient am</param>
        public CombFeedbackFilter(int m, double b0 = 1.0, double am = 1.0) : 
            base(new[] { b0 }, MakeDenominator(m, am))
        {
            _delay = m;
        }

        /// <summary>
        /// Static helper method
        /// </summary>
        /// <param name="m"></param>
        /// <param name="am"></param>
        /// <returns></returns>
        private static double[] MakeDenominator(int m, double am)
        {
            var kernel = new double[m + 1];
            kernel[0] = 1.0f;
            kernel[m] = am;

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

            var b0 = (float)B[0];
            var am = (float)A[_delay];

            for (var i = _delay; i < signal.Length; i++)
            {
                output[i] = b0 * input[i] - am * output[i - _delay];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
