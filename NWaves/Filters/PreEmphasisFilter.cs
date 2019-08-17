using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Filters
{
    /// <summary>
    /// Standard pre-emphasis FIR filter
    /// </summary>
    public class PreEmphasisFilter : FirFilter
    {
        /// <summary>
        /// Delay line
        /// </summary>
        private float _prevSample;

        /// <summary>
        /// Constructor computes simple 1st order kernel
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
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = _b[0] * sample + _b[1] * _prevSample;
            _prevSample = sample;

            return output;
        }

        /// <summary>
        /// Offline filtering
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
            var b1 = _b[1];

            _prevSample = 0;
            
            for (var i = 0; i < input.Length; i++)
            {
                var sample = input[i];
                output[i] = b0 * sample + b1 * _prevSample;
                _prevSample = sample;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Reset
        /// </summary>
        public override void Reset()
        {
            _prevSample = 0;
        }
    }
}
