using System;
using System.Collections.Generic;
using System.Numerics;
using NWaves.Utils;

namespace NWaves.Signals
{
    /// <summary>
    /// Static class providing extension methods for working with complex discrete signals.
    /// </summary>
    public static class ComplexDiscreteSignalExtensions
    {
        /// <summary>
        /// Create the delayed copy of <paramref name="signal"/> 
        /// by shifting it either to the right (positive <paramref name="delay"/>, e.g. Delay(1000)) 
        /// or to the left (negative <paramref name="delay"/>, e.g. Delay(-1000)).
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="delay">Delay (positive or negative number of delay samples)</param>
        public static ComplexDiscreteSignal Delay(this ComplexDiscreteSignal signal, int delay)
        {
            var length = signal.Length;

            if (delay <= 0)
            {
                delay = -delay;

                Guard.AgainstInvalidRange(delay, length, "Delay", "signal length");

                return new ComplexDiscreteSignal(
                                signal.SamplingRate,
                                signal.Real.FastCopyFragment(length - delay, delay),
                                signal.Imag.FastCopyFragment(length - delay, delay));
            }

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            signal.Real.FastCopyFragment(length, destinationOffset: delay),
                            signal.Imag.FastCopyFragment(length, destinationOffset: delay));
        }

        /// <summary>
        /// Superimpose signals <paramref name="signal1"/> and <paramref name="signal2"/>. 
        /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        public static ComplexDiscreteSignal Superimpose(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            ComplexDiscreteSignal superimposed;

            if (signal1.Length > signal2.Length)
            {
                superimposed = signal1.Copy();

                for (var i = 0; i < signal2.Length; i++)
                {
                    superimposed.Real[i] += signal2.Real[i];
                    superimposed.Imag[i] += signal2.Imag[i];
                }
            }
            else
            {
                superimposed = signal2.Copy();

                for (var i = 0; i < signal1.Length; i++)
                {
                    superimposed.Real[i] += signal1.Real[i];
                    superimposed.Imag[i] += signal1.Imag[i];
                }
            }

            return superimposed;
        }

        /// <summary>
        /// Concatenate <paramref name="signal1"/> and <paramref name="signal2"/>.
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        public static ComplexDiscreteSignal Concatenate(this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            return new ComplexDiscreteSignal(
                            signal1.SamplingRate,
                            signal1.Real.MergeWithArray(signal2.Real),
                            signal1.Imag.MergeWithArray(signal2.Imag));
        }

        /// <summary>
        /// Create the copy of <paramref name="signal"/> repeated <paramref name="n"/> times.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="n">Number of times to repeat <paramref name="signal"/></param>
        public static ComplexDiscreteSignal Repeat(this ComplexDiscreteSignal signal, int n)
        {
            Guard.AgainstNonPositive(n, "Number of repeat times");

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            signal.Real.RepeatArray(n),
                            signal.Imag.RepeatArray(n));
        }

        /// <summary>
        /// Amplify <paramref name="signal"/> by <paramref name="coeff"/> in-place.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="coeff">Amplification coefficient</param>
        public static void Amplify(this ComplexDiscreteSignal signal, double coeff)
        {
            for (var i = 0; i < signal.Length; i++)
            {
                signal.Real[i] *= coeff;
                signal.Imag[i] *= coeff;
            }
        }

        /// <summary>
        /// Attenuate <paramref name="signal"/> by <paramref name="coeff"/> in-place.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="coeff">Attenuation coefficient</param>
        public static void Attenuate(this ComplexDiscreteSignal signal, double coeff)
        {
            Guard.AgainstNonPositive(coeff, "Attenuation coefficient");

            signal.Amplify(1 / coeff);
        }

        /// <summary>
        /// Create new signal from first <paramref name="n"/> samples of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="n">Number of samples to copy</param>
        public static ComplexDiscreteSignal First(this ComplexDiscreteSignal signal, int n)
        {
            Guard.AgainstNonPositive(n, "Number of samples");
            Guard.AgainstExceedance(n, signal.Length, "Number of samples", "signal length");

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            signal.Real.FastCopyFragment(n),
                            signal.Imag.FastCopyFragment(n));
        }

        /// <summary>
        /// Create new signal from last <paramref name="n"/> samples of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="n">Number of samples to copy</param>
        public static ComplexDiscreteSignal Last(this ComplexDiscreteSignal signal, int n)
        {
            Guard.AgainstNonPositive(n, "Number of samples");
            Guard.AgainstExceedance(n, signal.Length, "Number of samples", "signal length");

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            signal.Real.FastCopyFragment(n, signal.Length - n),
                            signal.Imag.FastCopyFragment(n, signal.Imag.Length - n));
        }

        /// <summary>
        /// Create new zero-padded complex discrete signal of <paramref name="length"/> from <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="length">The length of a zero-padded signal.</param>
        public static ComplexDiscreteSignal ZeroPadded(this ComplexDiscreteSignal signal, int length)
        {
            if (length <= 0)
            {
                length = MathUtils.NextPowerOfTwo(signal.Length);
            }

            return new ComplexDiscreteSignal(
                            signal.SamplingRate,
                            signal.Real.PadZeros(length),
                            signal.Imag.PadZeros(length));
        }

        /// <summary>
        /// Perform the complex multiplication of <paramref name="signal1"/> and <paramref name="signal2"/> (with normalization by length).
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        public static ComplexDiscreteSignal Multiply(
            this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            var length = signal1.Length;

            var real = new double[length];
            var imag = new double[length];

            var real1 = signal1.Real;
            var imag1 = signal1.Imag;
            var real2 = signal2.Real;
            var imag2 = signal2.Imag;

            for (var i = 0; i < length; i++)
            {
                real[i] = real1[i] * real2[i] - imag1[i] * imag2[i];
                imag[i] = real1[i] * imag2[i] + imag1[i] * real2[i];
            }

            return new ComplexDiscreteSignal(signal1.SamplingRate, real, imag);
        }

        /// <summary>
        /// Perform the complex division of <paramref name="signal1"/> and <paramref name="signal2"/> (with normalization by length).
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        public static ComplexDiscreteSignal Divide(
            this ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            var length = signal1.Length;

            var real = new double[length];
            var imag = new double[length];

            var real1 = signal1.Real;
            var imag1 = signal1.Imag;
            var real2 = signal2.Real;
            var imag2 = signal2.Imag;

            for (var i = 0; i < length; i++)
            {
                var den = imag1[i] * imag1[i] + imag2[i] * imag2[i];
                real[i] = (real1[i] * real2[i] + imag1[i] * imag2[i]) / den;
                imag[i] = (real2[i] * imag1[i] - imag2[i] * real1[i]) / den;
            }

            return new ComplexDiscreteSignal(signal1.SamplingRate, real, imag);
        }

        /// <summary>
        /// Unwrap phases of complex-valued samples.
        /// </summary>
        /// <param name="phase">Phases</param>
        /// <param name="tolerance">Jump size</param>
        public static double[] Unwrap(this double[] phase, double tolerance = Math.PI)
        {
            return MathUtils.Unwrap(phase, tolerance);
        }

        /// <summary>
        /// Yield complex numbers as type <see cref="Complex"/> from <paramref name="signal"/> samples.
        /// </summary>
        /// <param name="signal">Complex discrete signal</param>
        public static IEnumerable<Complex> ToComplexNumbers(this ComplexDiscreteSignal signal)
        {
            for (var i = 0; i < signal.Length; i++)
            {
                yield return new Complex(signal.Real[i], signal.Imag[i]);
            }
        }
    }
}
