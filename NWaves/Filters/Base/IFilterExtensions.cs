using NWaves.Filters.Base64;
using NWaves.Signals;
using NWaves.Transforms;
using System;
using System.Linq;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Class providing extension methods for online filters.
    /// </summary>
    public static class IFilterExtensions
    {
        /// <summary>
        /// Filter data frame-wise.
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="input">Input block of samples</param>
        /// <param name="output">Block of filtered samples</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting index</param>
        /// <param name="outputPos">Output starting index</param>
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
        /// Filter entire <paramref name="signal"/> by processing each signal sample in a loop.
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="signal">Input signal</param>
        public static DiscreteSignal FilterOnline(this IOnlineFilter filter, DiscreteSignal signal)
        {
            var output = new float[signal.Length];
            var samples = signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = filter.Process(samples[i]);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Filter entire <paramref name="signal"/> by processing each signal sample in a loop.
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="signal">Input signal</param>
        public static double[] FilterOnline(this IOnlineFilter64 filter, double[] signal)
        {
            var output = new double[signal.Length];

            for (var i = 0; i < signal.Length; i++)
            {
                output[i] = filter.Process(signal[i]);
            }

            return output;
        }

        /// <summary>
        /// Calculate extra gain for filtering so that frequency response is normalized onto [0, 1] range.
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="fftSize">FFT size (for evaluating frequency response)</param>
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
        /// Filter entire <paramref name="signal"/> with extra <paramref name="gain"/>.
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="signal">Input signal</param>
        /// <param name="gain">Gain</param>
        public static DiscreteSignal ApplyTo(this IOnlineFilter filter,
                                             DiscreteSignal signal,
                                             float gain)
        {
            var output = signal.Samples.Select(s => gain * filter.Process(s));
            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Process one <paramref name="sample"/> of a signal with extra <paramref name="gain"/>.
        /// </summary>
        /// <param name="filter">Online filter</param>
        /// <param name="sample">Input sample</param>
        /// <param name="gain">Gain</param>
        public static float Process(this IOnlineFilter filter, float sample, float gain)
        {
            return gain * filter.Process(sample);
        }

#if DEBUG
        /// <summary>
        /// NOTE. For educational purposes and for testing online filtering.
        /// 
        /// Implementation of offline filtering in time domain frame-by-frame.
        /// 
        /// </summary>
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
#endif
    }
}
