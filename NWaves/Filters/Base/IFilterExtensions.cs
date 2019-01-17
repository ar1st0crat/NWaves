using NWaves.Signals;

namespace NWaves.Filters.Base
{
    public static class IFilterExtensions
    {
        /// <summary>
        /// Method implements online filtering (sample-by-sample, buffer-by-buffer)
        /// </summary>
        /// <param name="filter">Some filter</param>
        /// <param name="input">Input signal</param>
        /// <param name="method">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public static DiscreteSignal Process(this IOnlineFilter filter,
                                                  DiscreteSignal input,
                                                  FilteringMethod method = FilteringMethod.Auto)
        {
            var output = new float [input.Length];
            filter.Process(input.Samples, output, output.Length);
            return new DiscreteSignal(input.SamplingRate, output);
        }

        /// <summary>
        /// NOTE. For educational purposes and for testing online filtering.
        /// 
        /// Implementation of offline filtering in time domain frame-by-frame.
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="frameSize"></param>
        /// <param name="method"></param>
        /// <returns></returns>        
        public static DiscreteSignal OnlineChunks(this IOnlineFilter filter,
                                                       DiscreteSignal signal,
                                                       int frameSize = 4096,
                                                       FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            for (int i = 0; i + frameSize < input.Length; i += frameSize)
            {
                filter.Process(input, output, frameSize, i, i, method);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
