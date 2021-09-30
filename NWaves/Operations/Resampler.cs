using System;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Operations
{
    /// <summary>
    /// Represents signal resampler (sampling rate converter).
    /// </summary>
    public class Resampler
    {
        /// <summary>
        /// Gets or sets the order of lowpass anti-aliasing FIR filter 
        /// that will be created automatically if the filter is not specified explicitly. 
        /// By default, 101.
        /// </summary>
        public int MinResamplingFilterOrder { get; set; } = 101;

        /// <summary>
        /// Does interpolation of <paramref name="signal"/> followed by lowpass filtering.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="factor">Interpolation factor (e.g. factor=2 if 8000 Hz -> 16000 Hz)</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
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

            if (filter is null)
            {
                var filterSize = factor > MinResamplingFilterOrder / 2 ?
                                 2 * factor + 1 :
                                 MinResamplingFilterOrder;

                lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));
            }

            return lpFilter.ApplyTo(new DiscreteSignal(signal.SamplingRate * factor, output));
        }

        /// <summary>
        /// Does decimation of <paramref name="signal"/> preceded by lowpass filtering.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="factor">Decimation factor (e.g. factor=2 if 16000 Hz -> 8000 Hz)</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
        public DiscreteSignal Decimate(DiscreteSignal signal, int factor, FirFilter filter = null)
        {
            if (factor == 1)
            {
                return signal.Copy();
            }

            var filterSize = factor > MinResamplingFilterOrder / 2 ?
                             2 * factor + 1 :
                             MinResamplingFilterOrder;

            if (filter is null)
            {
                var lpFilter = new FirFilter(DesignFilter.FirWinLp(filterSize, 0.5f / factor));

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
        /// Does band-limited resampling of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="newSamplingRate">Desired sampling rate</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
        /// <param name="order">Order</param>
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

            if (g < 1 && filter is null)
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
        /// Does simple resampling of <paramref name="signal"/> (as the combination of interpolation and decimation).
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="up">Interpolation factor</param>
        /// <param name="down">Decimation factor</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
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

            if (filter is null)
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
