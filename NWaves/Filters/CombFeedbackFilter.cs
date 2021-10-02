using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Represents feedback comb filter:
    /// <code>
    ///     y[n] = b0 * x[n] - am * y[n - m]
    /// </code>
    /// </summary>
    public class CombFeedbackFilter : IirFilter
    {
        /// <summary>
        /// Delay (m).
        /// </summary>
        private readonly int _delay;

        /// <summary>
        /// Constructs <see cref="CombFeedbackFilter"/>.
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
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var b0 = _b[0];
            var am = _a[_delay];

            var output = b0 * sample - am * _delayLineA[_delayLineOffsetA];

            _delayLineA[_delayLineOffsetA] = output;

            if (--_delayLineOffsetA < 1)
            {
                _delayLineOffsetA = _denominatorSize - 1;
            }

            return output;
        }

        /// <summary>
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            if (method != FilteringMethod.Auto)
            {
                return base.ApplyTo(signal, method);
            }

            var input = signal.Samples;
            var output = new float[input.Length];

            var b0 = _b[0];
            var am = _a[_delay];

            for (int i = 0; i < _delay; i++)
            {
                output[i] = b0 * input[i];
            }
            for (int i = _delay, j = 0; i < signal.Length; i++, j++)
            {
                output[i] = b0 * input[i] - am * output[j];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Changes coefficients (preserving the state of the filter).
        /// </summary>
        /// <param name="b0">Coefficient b0</param>
        /// <param name="am">Coefficient am</param>
        public void Change(double b0, double am)
        {
            _b[0] = (float)b0;
            _a[_delay] = (float)am;
        }
    }
}
