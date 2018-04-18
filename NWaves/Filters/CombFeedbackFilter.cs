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
        public CombFeedbackFilter(int m, double b0 = 1.0, double am = 0.6) : base(MakeTf(m, b0, am))
        {
            _delay = m;
        }

        /// <summary>
        /// TF generator
        /// </summary>
        /// <param name="m">Delay</param>
        /// <param name="b0">Coefficient b0</param>
        /// <param name="am">Coefficient am</param>
        private static TransferFunction MakeTf(int m, double b0, double am)
        {
            var a = new double[m + 1];
            a[0] = 1.0;
            a[m] = am;

            return new TransferFunction(new [] {b0}, a);
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

            var b0 = _b32[0];
            var am = _a32[_delay];

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
        /// Online filtering (frame-by-frame)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var output = new float[input.Length];

            var b0 = _b32[0];
            var am = _a32[_delay];

            for (var n = 0; n < input.Length; n++)
            {
                output[n] = b0 * input[n] - am * _delayLineA[_delayLineOffsetA];

                _delayLineA[_delayLineOffsetA] = output[n];
                
                if (--_delayLineOffsetA < 1)
                {
                    _delayLineOffsetA = _delayLineA.Length - 1;
                }
            }

            return output;
        }
    }
}
