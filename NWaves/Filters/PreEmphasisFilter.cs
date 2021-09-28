using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Represents pre-emphasis FIR filter.
    /// </summary>
    public class PreEmphasisFilter : FirFilter
    {
        /// <summary>
        /// Delay line (consisting of one sample).
        /// </summary>
        private float _prevSample;

        /// <summary>
        /// Constructs <see cref="PreEmphasisFilter"/>.
        /// </summary>
        /// <param name="a">Pre-emphasis coefficient</param>
        /// <param name="normalize">Normalize freq response to unit gain</param>
        public PreEmphasisFilter(double a = 0.97, bool normalize = false) : base(new [] { 1, -a })
        {
            if (normalize)
            {
                var sum = (float)(1 + a);
                _b[0] /= sum;
                _b[1] /= sum;
            }
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = _b[0] * sample + _b[1] * _prevSample;
            _prevSample = sample;

            return output;
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            if (method != FilteringMethod.Auto)
            {
                return base.ApplyTo(signal, method);
            }

            var input = signal.Samples;
            var output = new float[input.Length + 1];

            var b0 = _b[0];
            var b1 = _b[1];

            _prevSample = 0;

            int i = 0;
            for (; i < input.Length; i++)
            {
                var sample = input[i];
                output[i] = b0 * sample + b1 * _prevSample;
                _prevSample = sample;
            }
            output[i] = b1 * _prevSample;

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public override void Reset()
        {
            _prevSample = 0;
        }
    }
}
