using NWaves.Filters.Base;
using NWaves.Signals;

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
        public CombFeedbackFilter(int m, double b0 = 1.0, double am = 0.6) : base(new float[1], new float[m + 1])
        {
            _a[0] = 1;
            _a[m] = (float)am;
            _b[0] = (float)b0;

            _delay = m;
        }

        /// <summary>
        /// Apply filter
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
            var output = new float[input.Length];

            var b0 = _b[0];
            var am = _a[_delay];

            for (var i = 0; i < _delay; i++)
            {
                output[i] = b0 * input[i];
            }
            for (var i = _delay; i < signal.Length; i++)
            {
                output[i] = b0 * input[i] - am * output[i - _delay];
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
            var am = _a[_delay];

            var output = b0 * sample - am * _delayLineA[_delayLineOffsetA];

            _delayLineA[_delayLineOffsetA] = output;

            if (--_delayLineOffsetA < 1)
            {
                _delayLineOffsetA = _delayLineA.Length - 1;
            }

            return output;
        }

        /// <summary>
        /// Change coefficients (preserving the state)
        /// </summary>
        /// <param name="b0"></param>
        /// <param name="am"></param>
        public void Change(double b0, double am)
        {
            _b[0] = (float)b0;
            _a[_delay] = (float)am;
        }
    }
}
