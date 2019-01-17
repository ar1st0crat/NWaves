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

            var b0 = _kernel32[0];
            var bm = _kernel32[_delay];

            for (var i = 0; i < _delay; i++)
            {
                output[i] = b0 * input[i];
            }
            for (var i = _delay; i < signal.Length; i++)
            {
                output[i] = b0 * input[i] + bm * input[i - _delay];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Online filtering
        /// </summary>
        /// <param name="input">Input block of samples</param>
        /// <param name="output">Block of filtered samples</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        /// <param name="method">General filtering strategy</param>
        public override void Process(float[] input,
                                     float[] output,
                                     int count,
                                     int inputPos = 0,
                                     int outputPos = 0,
                                     FilteringMethod method = FilteringMethod.Auto)
        {
            var b0 = _kernel32[0];
            var bm = _kernel32[_delay];

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                output[m] = b0 * input[n] + bm * _delayLine[_delayLineOffset];

                _delayLine[_delayLineOffset] = input[n];

                if (--_delayLineOffset < 1)
                {
                    _delayLineOffset = _delayLine.Length - 1;
                }
            }
        }
    }
}
