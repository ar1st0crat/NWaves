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
        public CombFeedbackFilter(int m, float b0 = 1.0f, float am = 1.0f) : 
            base(new[] {b0}, MakeDenominator(m, am))
        {
            _delay = m;
        }

        /// <summary>
        /// Static helper method
        /// </summary>
        /// <param name="m"></param>
        /// <param name="am"></param>
        /// <returns></returns>
        private static float[] MakeDenominator(int m, float am)
        {
            var kernel = new float[m + 1];
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

            FastCopy.ToExistingArray(input, output, _delay);

            var b0 = B[0];
            var am = A[_delay];

            for (var i = _delay; i < signal.Length; i++)
            {
                output[i] = b0 * input[i] - am * output[i - _delay];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
