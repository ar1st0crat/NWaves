using NWaves.Filters.Base;

namespace NWaves.Filters
{
    /// <summary>
    /// Standard pre-emphasis FIR filter
    /// </summary>
    public class PreEmphasisFilter : FirFilter
    {
        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly float _pre;

        /// <summary>
        /// Delay line
        /// </summary>
        private float _prevSample;

        /// <summary>
        /// Constructor computes simple 1st order kernel
        /// </summary>
        /// <param name="a">Pre-emphasis coefficient</param>
        public PreEmphasisFilter(double a = 0.97) : base(new [] { 1, -a })
        {
            _pre = -(float)a;
        }

        /// <summary>
        /// Online filtering (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            var output = sample + _pre * _prevSample;
            _prevSample = sample;

            return output;
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
