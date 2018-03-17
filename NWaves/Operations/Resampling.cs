using System;
using System.Linq;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Operations
{
    public static partial class Operation
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
        /// <returns></returns>
        public static DiscreteSignal Interpolate(DiscreteSignal signal, int factor)
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

            var filterSize = factor > MinResamplingFilterOrder / 2 ? 
                             2 * factor + 1 : 
                             MinResamplingFilterOrder;

            var lpFilter = DesignFilter.FirLp(filterSize, 0.5f / factor);

            return lpFilter.ApplyTo(new DiscreteSignal(signal.SamplingRate * factor, output));
        }

        /// <summary>
        /// Decimation preceded by low-pass filtering
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static DiscreteSignal Decimate(DiscreteSignal signal, int factor)
        {
            if (factor == 1)
            {
                return signal.Copy();
            }

            var filterSize = factor > MinResamplingFilterOrder / 2 ?
                             2 * factor + 1 :
                             MinResamplingFilterOrder;

            var lpFilter = DesignFilter.FirLp(filterSize, 0.5f / factor);

            signal = lpFilter.ApplyTo(signal);

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
        /// Simple resampling (as the combination of interpolation and decimation).
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="newSamplingRate"></param>
        /// <returns></returns>
        public static DiscreteSignal Resample(DiscreteSignal signal, int newSamplingRate)
        {
            if (newSamplingRate == signal.SamplingRate)
            {
                return signal.Copy();
            }

            var gcd = MathUtils.Gcd(signal.SamplingRate, newSamplingRate);

            var up = newSamplingRate / gcd;
            var down = signal.SamplingRate / gcd;

            if (up > 20 && down > 20)
            {
                return ResampleUpDown(signal, up, down);
            }

            var output = new float[signal.Length * up];

            var pos = 0;
            for (var i = 0; i < signal.Length; i++)
            {
                output[pos] = up * signal[i];
                pos += up;
            }

            var factor = Math.Max(up, down);
            var filterSize = factor > MinResamplingFilterOrder / 2 ?
                             8 * factor + 1 :
                             MinResamplingFilterOrder;

            var lpFilter = DesignFilter.FirLp(filterSize, 0.5f / factor);

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

        /// <summary>
        /// Resampling based on simple interpolation
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="up"></param>
        /// <param name="down"></param>
        /// <returns></returns>
        private static DiscreteSignal ResampleUpDown(DiscreteSignal signal, int up, int down)
        {
            var ratio = (float)up / down;

            var freq = ratio > 1 ? 0.5f / ratio : 0.5f * ratio;
            var lpFilter = DesignFilter.FirLp(MinResamplingFilterOrder, freq);

            var input = signal.Samples;
            var output = MathUtils.InterpolateLinear(
                                        Enumerable.Range(0, input.Length)   
                                                  .Select(s => s * ratio)
                                                  .ToArray(),
                                        input,                              
                                        Enumerable.Range(0, (int)(signal.Length * ratio) + 1)
                                                  .Select(s => (float)s)
                                                  .ToArray());

            return lpFilter.ApplyTo(new DiscreteSignal(signal.SamplingRate * up / down, output));
        }
    }
}
