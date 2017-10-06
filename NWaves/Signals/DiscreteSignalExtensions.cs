using System;

namespace NWaves.Signals
{
    /// <summary>
    /// In general, any finite DT signal is simply an array of data sampled at certain sampling rate.
    /// 
    /// This array of samples can be:
    ///     - delayed (shifted) by positive or negative number of samples
    ///     - superimposed with another array of samples (another signal)
    ///     - concatenated with another array of samples (another signal)
    ///     - repeated N times
    ///
    /// Note.
    /// Method implementations are LINQ-less and do Buffer.BlockCopy() for better performance.
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
            var length = signal.Samples.Length;

            double[] delayed;

            if (delay <= 0)
            {
                delay = -delay;

                if (delay >= length)
                {
                    throw new ArgumentException("Delay should not exceed the length of the signal!");
                }

                delayed = new double[length - delay];
                Buffer.BlockCopy(signal.Samples, delay * 8, delayed, 0, (length - delay) * 8);
            }
            else
            {
                delayed = new double[length + delay];
                Buffer.BlockCopy(signal.Samples, 0, delayed, delay * 8, length * 8);
            }

            return new DiscreteSignal(signal.SamplingRate, delayed);
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
                superimposed = signal1.Copy();

                for (var i = 0; i < signal2.Samples.Length; i++)
                {
                    superimposed[i] += signal2.Samples[i];
                }
            }
            else
            {
                superimposed = signal2.Copy();

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

            var concatenated = new double[signal1.Samples.Length + signal2.Samples.Length];
            Buffer.BlockCopy(signal1.Samples, 0, concatenated, 0, signal1.Samples.Length * 8);
            Buffer.BlockCopy(signal2.Samples, 0, concatenated, signal1.Samples.Length * 8, signal2.Samples.Length * 8);

            return new DiscreteSignal(signal1.SamplingRate, concatenated);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public static DiscreteSignal Repeat(this DiscreteSignal signal, int times)
        {
            if (times <= 0)
            {
                throw new ArgumentException("Number of repeat times must be at least once");
            }

            var repeated = new double[signal.Samples.Length * times];

            var offset = 0;
            for (var i = 0; i < times; i++)
            {
                Buffer.BlockCopy(signal.Samples, 0, repeated, offset * 8, signal.Samples.Length * 8);
                offset += signal.Samples.Length;
            }

            return new DiscreteSignal(signal.SamplingRate, repeated);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public static DiscreteSignal First(this DiscreteSignal signal, int sampleCount)
        {
            if (sampleCount <= 0 || sampleCount >= signal.Samples.Length)
            {
                throw new ArgumentException("Number of samples must be positive and must not exceed the signal length!");
            }

            var samples = new double[sampleCount];
            Buffer.BlockCopy(signal.Samples, 0, samples, 0, sampleCount * 8);

            return new DiscreteSignal(signal.SamplingRate, samples);
        }

        /// <summary>
        /// More or less efficient LINQ-less version.
        /// Skip() would require unnecessary enumeration.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public static DiscreteSignal Last(this DiscreteSignal signal, int sampleCount)
        {
            if (sampleCount <= 0 || sampleCount >= signal.Samples.Length)
            {
                throw new ArgumentException("Number of samples must be positive and must not exceed the signal length!");
            }

            var samples = new double[sampleCount];
            Buffer.BlockCopy(signal.Samples, (signal.Samples.Length - sampleCount) * 8, samples, 0, sampleCount * 8);

            return new DiscreteSignal(signal.SamplingRate, samples);
        }
    }
}
