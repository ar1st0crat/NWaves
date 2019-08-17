using NWaves.Filters.Base;
using NWaves.Signals;

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
        /// <param name="normalize"></param>
        public CombFeedforwardFilter(int m, double b0 = 1, double bm = 0.5, bool normalize = true)
            : base(MakeKernel(m, b0, bm, normalize))
        {
            _delay = m;
        }

        /// <summary>
        /// Kernel generator
        /// </summary>
        /// <param name="m">Delay</param>
        /// <param name="b0">Coefficient b0</param>
        /// <param name="bm">Coefficient bm</param>
        /// <param name="normalize"></param>
        private static float[] MakeKernel(int m, double b0, double bm, bool normalize)
        {
            var kernel = new float[m + 1];
            kernel[0] = (float)b0;
            kernel[m] = (float)bm;

            if (normalize)
            {
                var sum = (float)(b0 + bm);
                kernel[0] /= sum;
                kernel[m] /= sum;
            }

            return kernel;
        }
        
        /// <summary>
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var b0 = _b[0];
            var bm = _b[_delay];

            var output = b0 * sample + bm * _delayLine[_delayLineOffset];

            _delayLine[_delayLineOffset] = sample;

            if (--_delayLineOffset < 1)
            {
                _delayLineOffset = _kernelSize - 1;
            }

            return output;
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
            var output = new float[input.Length + _kernelSize - 1];

            var b0 = _b[0];
            var bm = _b[_delay];

            int i = 0, j = 0;

            for (; i < _delay; i++)
            {
                output[i] = b0 * input[i];
            }
            for (; i < signal.Length; i++, j++)
            {
                output[i] = b0 * input[i] + bm * input[j];
            }
            for (; i < output.Length; i++, j++)
            {
                output[i] = bm * input[j];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Change coefficients (preserving the state)
        /// </summary>
        /// <param name="b0"></param>
        /// <param name="bm"></param>
        public void Change(double b0, double bm)
        {
            _b[0] = (float)b0;
            _b[_delay] = (float)bm;
        }
    }
}
