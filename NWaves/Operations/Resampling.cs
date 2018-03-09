using System;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// The order of FIR LP resampling filter (minimally required).
        /// This constant was chosen empirically and should be used for simple up/down ratios.
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

            var output = new double[signal.Length * factor];

            var pos = 0;
            for (var i = 0; i < signal.Length; i++)
            {
                output[pos] = factor * signal[i];
                pos += factor;
            }

            var filterSize = factor > MinResamplingFilterOrder / 2 ? 
                             2 * factor + 1 : 
                             MinResamplingFilterOrder;

            var lpFilter = DesignFilter.FirLp(filterSize, 0.5 / factor);

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

            var lpFilter = DesignFilter.FirLp(filterSize, 0.5 / factor);

            signal = lpFilter.ApplyTo(signal);

            var output = new double[signal.Length / factor];

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

            var output = new double[signal.Length * up];

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

            var lpFilter = DesignFilter.FirLp(filterSize, 0.5 / factor);

            var upsampled = lpFilter.ApplyTo(new DiscreteSignal(signal.SamplingRate * up, output));

            output = new double[upsampled.Length / down];

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
