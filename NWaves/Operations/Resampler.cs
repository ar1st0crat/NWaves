using System;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Operations
{
    /// <summary>
    /// Class responsible for sampling rate conversion
    /// </summary>
    public class Resampler
    {
        /// <summary>
        /// The order of FIR LP resampling filter (minimally required).
        /// This constant should be used for simple up/down ratios.
        /// </summary>
        private const int MinResamplingFilterOrder = 101;

        /// <summary>
        /// Interpolation followed by low-pass filtering
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="factor"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DiscreteSignal Interpolate(DiscreteSignal signal, int factor, FirFilter filter = null)
        {
            if (factor == 1)
            {
                return signal.Copy();
            }

            var output = new float[signal.Length * factor];

            var pos = 0;
            for (var i = 0; i < signal.Length; i++)
            {
                output[pos] = factor * signal[i];
                pos += factor;
            }

            var lpFilter = filter;

            if (filter == null)
            {
                var filterSize = factor > MinResamplingFilterOrder / 2 ?
                                 2 * factor + 1 :
                                 MinResamplingFilterOrder;

                lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));
            }

            return lpFilter.ApplyTo(new DiscreteSignal(signal.SamplingRate * factor, output));
        }

        /// <summary>
        /// Decimation preceded by low-pass filtering
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="factor"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DiscreteSignal Decimate(DiscreteSignal signal, int factor, FirFilter filter = null)
        {
            if (factor == 1)
            {
                return signal.Copy();
            }

            var filterSize = factor > MinResamplingFilterOrder / 2 ?
                             2 * factor + 1 :
                             MinResamplingFilterOrder;

            var lpFilter = filter;

            if (filter == null)
            {
                lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));

                signal = lpFilter.ApplyTo(signal);
            }

            var output = new float[signal.Length / factor];

            var pos = 0;
            for (var i = 0; i < output.Length; i++)
            {
                output[i] = signal[pos];
                pos += factor;
            }

            return new DiscreteSignal(signal.SamplingRate / factor, output);
        }

        /// <summary>
        /// Band-limited resampling
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="newSamplingRate"></param>
        /// <param name="filter"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public DiscreteSignal Resample(DiscreteSignal signal,
                                       int newSamplingRate,
                                       FirFilter filter = null,
                                       int order = 15)
        {
            if (signal.SamplingRate == newSamplingRate)
            {
                return signal.Copy();
            }

            var g = (float) newSamplingRate / signal.SamplingRate;

            var input = signal.Samples;
            var output = new float[(int)(input.Length * g)];

            if (g < 1 && filter == null)
            {
                filter = new FirFilter(DesignFilter.FirWinLp(MinResamplingFilterOrder, g / 2));

                input = filter.ApplyTo(signal).Samples;
            }

            var step = 1 / g;

            for (var n = 0; n < output.Length; n++)
            {
                var x = n * step;

                for (var i = -order; i < order; i++)
                {
                    var j = (int) Math.Floor(x) - i;

                    if (j < 0 || j >= input.Length)
                    {
                        continue;
                    }

                    var t = x - j;
                    float w = (float) (0.5 * (1.0 + Math.Cos(t / order * Math.PI)));    // Hann window
                    float sinc = (float) MathUtils.Sinc(t);                             // Sinc function
                    output[n] += w * sinc * input[j];
                }
            }

            return new DiscreteSignal(newSamplingRate, output);
        }

        /// <summary>
        /// Simple resampling as the combination of interpolation and decimation.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="up"></param>
        /// <param name="down"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public DiscreteSignal ResampleUpDown(DiscreteSignal signal, int up, int down, FirFilter filter = null)
        {
            if (up == down)
            {
                return signal.Copy();
            }

            var newSamplingRate = signal.SamplingRate * up / down;

            if (up > 20 && down > 20)
            {
                return Resample(signal, newSamplingRate, filter);
            }

            var output = new float[signal.Length * up];

            var pos = 0;
            for (var i = 0; i < signal.Length; i++)
            {
                output[pos] = up * signal[i];
                pos += up;
            }

            var lpFilter = filter;

            if (filter == null)
            {
                var factor = Math.Max(up, down);
                var filterSize = factor > MinResamplingFilterOrder / 2 ?
                                 8 * factor + 1 :
                                 MinResamplingFilterOrder;

                lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));
            }

            var upsampled = lpFilter.ApplyTo(new DiscreteSignal(signal.SamplingRate * up, output));

            output = new float[upsampled.Length / down];

            pos = 0;
            for (var i = 0; i < output.Length; i++)
            {
                output[i] = upsampled[pos];
                pos += down;
            }

            return new DiscreteSignal(newSamplingRate, output);
        }
    }
}
