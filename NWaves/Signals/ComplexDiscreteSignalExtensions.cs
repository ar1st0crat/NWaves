using System;
using NWaves.Utils;

namespace NWaves.Signals
{
    /// <summary>
    /// Any finite complex DT signal is simply two arrays of data (real and imaginary parts)
    /// sampled at certain sampling rate.
    /// 
    /// This arrays of samples can be:
    ///     - delayed (shifted) by positive or negative number of samples
    ///     - superimposed with another arrays of samples (another signal)
    ///     - concatenated with another arrays of samples (another signal)
    ///     - repeated N times
    /// 
    /// Note.
    /// Method implementations are LINQ-less and do Buffer.BlockCopy() for better performance.
    /// </summary>
    public static class ComplexDiscreteSignalExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal Delay(this ComplexDiscreteSignal signal, int delay)
        {
            var length = signal.Real.Length;

            if (delay <= 0)
            {
                delay = -delay;

                if (delay >= length)
                {
                    throw new ArgumentException("Delay can not exceed the length of the signal!");
                }

                return new ComplexDiscreteSignal(
                                signal.SamplingRate,
                                FastCopy.ArrayFragment(signal.Real, length - delay, delay),
                                FastCopy.ArrayFragment(signal.Imag, length - delay, delay));
            }

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            FastCopy.ArrayFragment(signal.Real, length, destinationOffset: delay),
                            FastCopy.ArrayFragment(signal.Imag, length, destinationOffset: delay));
        }

        /// <summary>
        /// Method superimposes signal1 with signal2.
        /// 
        /// If the size of one of the arrays is smaller, then it's padded with zeros.
        /// </summary>
        /// <param name="signal1">Object signal</param>
        /// <param name="signal2">Argument signal</param>
        /// <returns></returns>
        public static ComplexDiscreteSignal Superimpose(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            if (signal1.SamplingRate != signal2.SamplingRate)
            {
                throw new ArgumentException("Sampling rates must be the same!");
            }

            ComplexDiscreteSignal superimposed;

            if (signal1.Real.Length > signal2.Real.Length)
            {
                superimposed = signal1.Copy();

                for (var i = 0; i < signal2.Real.Length; i++)
                {
                    superimposed.Real[i] += signal2.Real[i];
                    superimposed.Imag[i] += signal2.Imag[i];
                }
            }
            else
            {
                superimposed = signal2.Copy();

                for (var i = 0; i < signal1.Real.Length; i++)
                {
                    superimposed.Real[i] += signal1.Real[i];
                    superimposed.Imag[i] += signal1.Imag[i];
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
        public static ComplexDiscreteSignal Concatenate(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            if (signal1.SamplingRate != signal2.SamplingRate)
            {
                throw new ArgumentException("Sampling rates must be the same!");
            }

            return new ComplexDiscreteSignal(
                            signal1.SamplingRate,
                            FastCopy.MergeArrays(signal1.Real, signal2.Real),
                            FastCopy.MergeArrays(signal1.Imag, signal2.Imag));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal Repeat(this ComplexDiscreteSignal signal, int times)
        {
            if (times <= 0)
            {
                throw new ArgumentException("Number of repeat times must be at least once");
            }

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            FastCopy.RepeatArray(signal.Real, times),
                            FastCopy.RepeatArray(signal.Imag, times));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal First(this ComplexDiscreteSignal signal, int sampleCount)
        {
            if (sampleCount <= 0 || sampleCount >= signal.Real.Length)
            {
                throw new ArgumentException("Number of samples must be positive and must not exceed the signal length!");
            }
            
            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            FastCopy.ArrayFragment(signal.Real, sampleCount),
                            FastCopy.ArrayFragment(signal.Imag, sampleCount));
        }

        /// <summary>
        /// More or less efficient LINQ-less version.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal Last(this ComplexDiscreteSignal signal, int sampleCount)
        {
            if (sampleCount <= 0 || sampleCount >= signal.Real.Length)
            {
                throw new ArgumentException("Number of samples must be positive and must not exceed the signal length!");
            }

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            FastCopy.ArrayFragment(signal.Real, sampleCount, signal.Real.Length - sampleCount),
                            FastCopy.ArrayFragment(signal.Imag, sampleCount, signal.Imag.Length - sampleCount));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal Multiply(
            this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            var length = signal1.Real.Length;

            var real = new double[length];
            var imag = new double[length];

            var real1 = signal1.Real;
            var imag1 = signal1.Imag;
            var real2 = signal2.Real;
            var imag2 = signal2.Imag;

            for (var i = 0; i < length; i++)
            {
                real[i] = (real1[i] * real2[i] - imag1[i] * imag2[i]) / length;
                imag[i] = (real1[i] * imag2[i] + imag1[i] * real2[i]) / length;
            }

            return new ComplexDiscreteSignal(signal1.SamplingRate, real, imag);
        }
    }
}
