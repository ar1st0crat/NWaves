using NWaves.Filters.Base;
using NWaves.Utils;
using System;

namespace NWaves.Filters.Adaptive
{
    /// <summary>
    /// Base abstract class for adaptive filters
    /// </summary>
    public abstract class AdaptiveFilter : FirFilter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="order"></param>
        public AdaptiveFilter(int order) : base(new float[order])
        {
            Array.Resize(ref _delayLine, _kernelSize * 2);  // trick for better performance
        }

        /// <summary>
        /// Init weights
        /// </summary>
        /// <param name="weights"></param>
        public void Init(float[] weights)
        {
            Guard.AgainstInequality(_kernelSize, weights.Length, "Filter order", "Weights array size");
            ChangeKernel(weights);
        }

        /// <summary>
        /// Process one sample of input signal and one sample of desired signal
        /// </summary>
        /// <param name="input"></param>
        /// <param name="desired"></param>
        /// <returns></returns>
        public abstract float Process(float input, float desired);
    }
}
