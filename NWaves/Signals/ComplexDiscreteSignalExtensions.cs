using System;

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

            double[] realDelayed;
            double[] imagDelayed;

            if (delay <= 0)
            {
                delay = -delay;

                if (delay >= length)
                {
                    throw new ArgumentException("Delay should not exceed the length of the signal!");
                }

                realDelayed = new double[length - delay];
                Buffer.BlockCopy(signal.Real, delay * 8, realDelayed, 0, (length - delay) * 8);

                imagDelayed = new double[length - delay];
                Buffer.BlockCopy(signal.Imag, delay * 8, imagDelayed, 0, (length - delay) * 8);
            }
            else
            {
                realDelayed = new double[length + delay];
                Buffer.BlockCopy(signal.Real, 0, realDelayed, delay * 8, length * 8);

                imagDelayed = new double[length + delay];
                Buffer.BlockCopy(signal.Imag, 0, imagDelayed, delay * 8, length * 8);
            }

            return new ComplexDiscreteSignal(signal.SamplingRate, realDelayed, imagDelayed);
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
                throw new ArgumentException("Sampling rates should be the same!");
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
                throw new ArgumentException("Sampling rates should be the same!");
            }

            var length = signal1.Real.Length + signal2.Real.Length;

            var realConcatenated = new double[length];
            Buffer.BlockCopy(signal1.Real, 0, realConcatenated, 0, signal1.Real.Length * 8);
            Buffer.BlockCopy(signal2.Real, 0, realConcatenated, signal1.Real.Length * 8, signal2.Real.Length * 8);

            var imagConcatenated = new double[length];
            Buffer.BlockCopy(signal1.Imag, 0, imagConcatenated, 0, signal1.Imag.Length * 8);
            Buffer.BlockCopy(signal2.Imag, 0, imagConcatenated, signal1.Imag.Length * 8, signal2.Imag.Length * 8);

            return new ComplexDiscreteSignal(signal1.SamplingRate, realConcatenated, imagConcatenated);
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

            var realRepeated = new double[signal.Real.Length * times];
            var imagRepeated = new double[signal.Real.Length * times];

            var offset = 0;
            for (var i = 0; i < times; i++)
            {
                Buffer.BlockCopy(signal.Real, 0, realRepeated, offset * 8, signal.Real.Length * 8);
                Buffer.BlockCopy(signal.Imag, 0, imagRepeated, offset * 8, signal.Imag.Length * 8);
                offset += signal.Real.Length;
            }

            return new ComplexDiscreteSignal(signal.SamplingRate, realRepeated, imagRepeated);
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

            var realSamples = new double[sampleCount];
            Buffer.BlockCopy(signal.Real, 0, realSamples, 0, sampleCount * 8);

            var imagSamples = new double[sampleCount];
            Buffer.BlockCopy(signal.Imag, 0, imagSamples, 0, sampleCount * 8);
            
            return new ComplexDiscreteSignal(signal.SamplingRate, realSamples, imagSamples);
        }

        /// <summary>
        /// More or less efficient LINQ-less version.
        /// Skip() would require unnecessary enumeration.
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

            var realSamples = new double[sampleCount];
            Buffer.BlockCopy(signal.Real, (signal.Real.Length - sampleCount) * 8, realSamples, 0, sampleCount * 8);

            var imagSamples = new double[sampleCount];
            Buffer.BlockCopy(signal.Imag, (signal.Imag.Length - sampleCount) * 8, imagSamples, 0, sampleCount * 8);

            return new ComplexDiscreteSignal(signal.SamplingRate, realSamples, imagSamples);
        }
    }
}
