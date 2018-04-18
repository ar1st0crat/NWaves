using NWaves.Filters.Base;

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
        private float _prev;

        /// <summary>
        /// Constructor computes simple 1st order kernel
        /// </summary>
        /// <param name="a">Pre-emphasis coefficient</param>
        public PreEmphasisFilter(double a = 0.97) : base(new [] {1, -a})
        {
        }

        /// <summary>
        /// Online filtering
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public override float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var output = new float[input.Length];

            var b = _kernel32;

            for (var n = 0; n < input.Length; n++)
            {
                output[n] = b[0] * input[n] + b[1] * _prev;
                _prev = input[n];
            }

            return output;
        }

        /// <summary>
        /// Reset
        /// </summary>
        public override void Reset()
        {
            _prev = 0;
        }
    }
}
