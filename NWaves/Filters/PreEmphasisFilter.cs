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
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = _kernel32[0] * sample + _kernel32[1] * _prev;
            _prev = sample;

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
