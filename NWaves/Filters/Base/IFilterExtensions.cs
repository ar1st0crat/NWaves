using NWaves.Signals;
using NWaves.Transforms;
using System;
using System.Linq;

namespace NWaves.Filters.Base
{
    public static class IFilterExtensions
    {
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
        /// Calculate filtering gain so that frequency response is normalized onto [0, 1] range.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="fftSize"></param>
        /// <returns>Gain for filtering operations</returns>
        public static float EstimateGain(this IOnlineFilter filter, int fftSize = 512)
        {
            var unit = DiscreteSignal.Unit(fftSize);
            
            // get impulse response

            var response = unit.Samples.Select(s => filter.Process(s)).ToArray();

            // get frequency response

            var spectrum = new float[fftSize / 2 + 1];
            var fft = new RealFft(fftSize);
            fft.MagnitudeSpectrum(response, spectrum);

            return 1 / spectrum.Max(s => Math.Abs(s));
        }

        /// <summary>
        /// Filter signal with additional gain
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="input">Input signal</param>
        /// <param name="gain">Gain</param>
        /// <returns>Filtered signal</returns>
        public static DiscreteSignal ApplyTo(this IOnlineFilter filter,
                                             DiscreteSignal input,
                                             float gain)
        {
            var output = input.Samples.Select(s => gain * filter.Process(s));
            return new DiscreteSignal(input.SamplingRate, output);
        }

        /// <summary>
        /// Process one sample of a signal with additional gain
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="sample">Input sample</param>
        /// <param name="gain">Gain</param>
        /// <returns></returns>
        public static float Process(this IOnlineFilter filter, float sample, float gain)
        {
            return gain * filter.Process(sample);
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
        public static DiscreteSignal ProcessChunks(this IOnlineFilter filter,
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
