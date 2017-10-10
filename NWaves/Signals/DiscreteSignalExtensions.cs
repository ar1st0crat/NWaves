using System;
using NWaves.Utils;

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

            if (delay <= 0)
            {
                delay = -delay;

                if (delay >= length)
                {
                    throw new ArgumentException("Delay can not exceed the length of the signal!");
                }

                return new DiscreteSignal(
                                signal.SamplingRate,
                                FastCopy.ArrayFragment(signal.Samples, length - delay, delay));
            }
            
            return new DiscreteSignal(
                            signal.SamplingRate, 
                            FastCopy.ArrayFragment(signal.Samples, length, destinationOffset: delay));
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
                throw new ArgumentException("Sampling rates must be the same!");
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
                throw new ArgumentException("Sampling rates must be the same!");
            }

            return new DiscreteSignal(
                            signal1.SamplingRate, 
                            FastCopy.MergeArrays(signal1.Samples, signal2.Samples));
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

            return new DiscreteSignal(
                            signal.SamplingRate,
                            FastCopy.RepeatArray(signal.Samples, times));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public static DiscreteSignal First(this DiscreteSignal signal, int sampleCount)
        {
            if (sampleCount <= 0 || sampleCount > signal.Samples.Length)
            {
                throw new ArgumentException("Number of samples must be positive and must not exceed the signal length!");
            }
            
            return new DiscreteSignal(
                            signal.SamplingRate,
                            FastCopy.ArrayFragment(signal.Samples, sampleCount));
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

            return new DiscreteSignal(
                            signal.SamplingRate, 
                            FastCopy.ArrayFragment(signal.Samples, sampleCount, signal.Samples.Length - sampleCount));
        }

        /// <summary>
        /// Returns own complex-valued copy
        /// </summary>
        /// <param name="signal">Real-valued signal</param>
        /// <param name="size">New size of complex-valued signal</param>
        /// <returns>Corresponding complex-valued signal</returns>
        public static ComplexDiscreteSignal ToComplex(this DiscreteSignal signal, int size = -1)
        {
            return size == -1 ? 
                new ComplexDiscreteSignal(signal.SamplingRate, FastCopy.EntireArray(signal.Samples)) : 
                new ComplexDiscreteSignal(signal.SamplingRate, FastCopy.PadZeros(signal.Samples, size));
        }
    }
}
