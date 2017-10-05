using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals
{
    /// <summary>
    /// 
    /// In general, any finite DT signal is simply an array of data sampled at certain sampling rate.
    /// 
    /// This array of samples can be:
    ///     - delayed (shifted) by positive or negative number of samples
    ///     - superimposed with another array of samples (another signal)
    ///     - concatenated with another array of samples (another signal)
    ///     - repeated N times
    /// 
    /// </summary>
    public static class DiscreteSignalExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static DiscreteSignal Delay(this DiscreteSignal signal, int delay)
        {
            if (delay <= 0)
            {
                delay = -delay;

                if (delay >= signal.Samples.Length)
                {
                    throw new ArgumentException("Delay should not exceed the length of the signal!");
                }
                return new DiscreteSignal(signal.Samples.Skip(delay), signal.SamplingRate);
            }

            var delayed = new List<double>();
            delayed.AddRange(Enumerable.Repeat(0.0, delay));
            delayed.AddRange(signal.Samples);

            return new DiscreteSignal(delayed, signal.SamplingRate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal Superimpose(this DiscreteSignal signal1, DiscreteSignal signal2)
        {
            if (signal1.SamplingRate != signal2.SamplingRate)
            {
                throw new ArgumentException("Sampling rates should be the same!");
            }

            DiscreteSignal superimposed;

            if (signal1.Samples.Length > signal2.Samples.Length)
            {
                superimposed = new DiscreteSignal(signal1.Samples, signal1.SamplingRate);

                for (var i = 0; i < signal2.Samples.Length; i++)
                {
                    superimposed[i] += signal2.Samples[i];
                }
            }
            else
            {
                superimposed = new DiscreteSignal(signal2.Samples, signal2.SamplingRate);

                for (var i = 0; i < signal1.Samples.Length; i++)
                {
                    superimposed[i] += signal1.Samples[i];
                }
            }

            return superimposed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal Concatenate(this DiscreteSignal signal1, DiscreteSignal signal2)
        {
            if (signal1.SamplingRate != signal2.SamplingRate)
            {
                throw new ArgumentException("Sampling rates should be the same!");
            }

            var concatenated = new List<double>();
            concatenated.AddRange(signal1.Samples);
            concatenated.AddRange(signal2.Samples);

            return new DiscreteSignal(concatenated, signal1.SamplingRate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public static DiscreteSignal Repeat(this DiscreteSignal signal, int times)
        {
            var repeated = new List<double>();
            for (var i = 0; i < times; i++)
            {
                repeated.AddRange(signal.Samples);
            }

            return new DiscreteSignal(repeated, signal.SamplingRate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public static DiscreteSignal First(this DiscreteSignal signal, int sampleCount)
        {
            return new DiscreteSignal(signal.Samples.Take(sampleCount), signal.SamplingRate);
        }

        /// <summary>
        /// More or less efficient LINQ-less version.
        /// Skip() would require more resources.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public static DiscreteSignal Last(this DiscreteSignal signal, int sampleCount)
        {
            var samples = new double[sampleCount];
            signal.Samples.CopyTo(samples, signal.Samples.Length - sampleCount);

            return new DiscreteSignal(samples, signal.SamplingRate);
        }
    }
}
