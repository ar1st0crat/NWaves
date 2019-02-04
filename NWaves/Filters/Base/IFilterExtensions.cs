using NWaves.Signals;

namespace NWaves.Filters.Base
{
    public static class IFilterExtensions
    {
        /// <summary>
        /// Method implements online filtering for discrete signals
        /// </summary>
        /// <param name="filter">Some filter</param>
        /// <param name="input">Input signal</param>
        /// <param name="method">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public static DiscreteSignal Process(this IOnlineFilter filter,
                                                  DiscreteSignal input)
        {
            var output = new float [input.Length];
            filter.Process(input.Samples, output, output.Length);
            return new DiscreteSignal(input.SamplingRate, output);
        }

        /// <summary>
        /// Method implements online filtering (frame-by-frame)
        /// </summary>
        /// <param name="input">Input block of samples</param>
        /// <param name="output">Block of filtered samples</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        public static void Process(this IOnlineFilter filter,
                                   float[] input,
                                   float[] output,
                                   int count = 0,
                                   int inputPos = 0,
                                   int outputPos = 0)
        {
            if (count <= 0)
            {
                count = input.Length;
            }

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                output[m] = filter.Process(input[n]);
            }
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
                                                       int frameSize = 4096)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            var i = 0;
            for (; i + frameSize < input.Length; i += frameSize)
            {
                filter.Process(input, output, frameSize, i, i);
            }

            // process last chunk
            filter.Process(input, output, input.Length - i, i, i);

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
