using System;
using System.Linq;
using NWaves.Utils;

namespace NWaves.Signals
{
    /// <summary>
    /// Provides extension methods for working with <see cref="DiscreteSignal"/> objects.
    /// </summary>
    public static class DiscreteSignalExtensions
    {
        // Note.
        // Method implementations are LINQ-less and leverage FastCopy() for better performance.

        /// <summary>
        /// Creates the delayed copy of <paramref name="signal"/> 
        /// by shifting it either to the right (positive <paramref name="delay"/>, e.g. Delay(1000)) 
        /// or to the left (negative <paramref name="delay"/>, e.g. Delay(-1000)).
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="delay">Delay (positive or negative number of delay samples)</param>
        public static DiscreteSignal Delay(this DiscreteSignal signal, int delay)
        {
            var length = signal.Length;

            if (delay <= 0)
            {
                delay = -delay;

                Guard.AgainstInvalidRange(delay, length, "Delay", "signal length");

                return new DiscreteSignal(
                                signal.SamplingRate,
                                signal.Samples.FastCopyFragment(length - delay, delay));
            }
            
            return new DiscreteSignal(
                            signal.SamplingRate,
                            signal.Samples.FastCopyFragment(length, destinationOffset: delay));
        }

        /// <summary>
        /// Superimposes signals <paramref name="signal1"/> and <paramref name="signal2"/>. 
        /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        public static DiscreteSignal Superimpose(this DiscreteSignal signal1, DiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            DiscreteSignal superimposed;

            if (signal1.Length >= signal2.Length)
            {
                superimposed = signal1.Copy();

                for (var i = 0; i < signal2.Length; i++)
                {
                    superimposed[i] += signal2.Samples[i];
                }
            }
            else
            {
                superimposed = signal2.Copy();

                for (var i = 0; i < signal1.Length; i++)
                {
                    superimposed[i] += signal1.Samples[i];
                }
            }

            return superimposed;
        }

        /// <summary>
        /// Superimposes <paramref name="signal2"/> and <paramref name="signal1"/> multiple times at given <paramref name="positions"/>.
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        /// <param name="positions">Positions (indices) where to insert <paramref name="signal2"/></param>
        public static DiscreteSignal SuperimposeMany(this DiscreteSignal signal1, DiscreteSignal signal2, int[] positions)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            var totalLength = Math.Max(signal1.Length, signal2.Length + positions.Max());

            DiscreteSignal superimposed = new DiscreteSignal(signal1.SamplingRate, totalLength);
            signal1.Samples.FastCopyTo(superimposed.Samples, signal1.Length);

            for (var p = 0; p < positions.Length; p++)
            {
                var offset = positions[p];

                for (var i = 0; i < signal2.Length; i++)
                {
                    superimposed[offset + i] += signal2.Samples[i];
                }
            }

            return superimposed;
        }

        /// <summary>
        /// Subtracts <paramref name="signal2"/> from <paramref name="signal1"/>. 
        /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        public static DiscreteSignal Subtract(this DiscreteSignal signal1, DiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            DiscreteSignal subtracted;

            if (signal1.Length >= signal2.Length)
            {
                subtracted = signal1.Copy();

                for (var i = 0; i < signal2.Length; i++)
                {
                    subtracted[i] -= signal2.Samples[i];
                }
            }
            else
            {
                subtracted = new DiscreteSignal(signal2.SamplingRate, signal2.Length);

                for (var i = 0; i < signal1.Length; i++)
                {
                    subtracted[i] = signal1.Samples[i] - signal2.Samples[i];
                }
                for (var i = signal1.Length; i < signal2.Length; i++)
                {
                    subtracted[i] = -signal2.Samples[i];
                }
            }

            return subtracted;
        }

        /// <summary>
        /// Concatenates <paramref name="signal1"/> and <paramref name="signal2"/>.
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        public static DiscreteSignal Concatenate(this DiscreteSignal signal1, DiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate,
                                        "Sampling rate of signal1", "sampling rate of signal2");

            return new DiscreteSignal(
                            signal1.SamplingRate,
                            signal1.Samples.MergeWithArray(signal2.Samples));
        }

        /// <summary>
        /// Creates the copy of <paramref name="signal"/> repeated <paramref name="n"/> times.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="n">Number of times to repeat <paramref name="signal"/></param>
        public static DiscreteSignal Repeat(this DiscreteSignal signal, int n)
        {
            Guard.AgainstNonPositive(n, "Number of repeat times");
            
            return new DiscreteSignal(
                            signal.SamplingRate,
                            signal.Samples.RepeatArray(n));
        }

        /// <summary>
        /// Amplifies <paramref name="signal"/> by <paramref name="coeff"/> in-place.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="coeff">Amplification coefficient</param>
        public static void Amplify(this DiscreteSignal signal, float coeff)
        {
            for (var i = 0; i < signal.Length; i++)
            {
                signal[i] *= coeff;
            }
        }

        /// <summary>
        /// Attenuates <paramref name="signal"/> by <paramref name="coeff"/> in-place.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="coeff">Attenuation coefficient</param>
        public static void Attenuate(this DiscreteSignal signal, float coeff)
        {
            Guard.AgainstNonPositive(coeff, "Attenuation coefficient");

            signal.Amplify(1 / coeff);
        }

        /// <summary>
        /// Reverses <paramref name="signal"/> in-place.
        /// </summary>
        public static void Reverse(this DiscreteSignal signal)
        {
            var samples = signal.Samples;

            for (int i = 0, j = samples.Length - 1; i < samples.Length / 2; i++, j--)
            {
                var tmp = samples[i];
                samples[i] = samples[j];
                samples[j] = tmp;
            }
        }

        /// <summary>
        /// Creates new signal from first <paramref name="n"/> samples of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="n">Number of samples to copy</param>
        public static DiscreteSignal First(this DiscreteSignal signal, int n)
        {
            Guard.AgainstNonPositive(n, "Number of samples");
            Guard.AgainstExceedance(n, signal.Length, "Number of samples", "signal length");
            
            return new DiscreteSignal(
                            signal.SamplingRate,
                            signal.Samples.FastCopyFragment(n));
        }

        /// <summary>
        /// Creates new signal from last <paramref name="n"/> samples of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="n">Number of samples to copy</param>
        public static DiscreteSignal Last(this DiscreteSignal signal, int n)
        {
            Guard.AgainstNonPositive(n, "Number of samples");
            Guard.AgainstExceedance(n, signal.Length, "Number of samples", "signal length");

            return new DiscreteSignal(
                            signal.SamplingRate,
                            signal.Samples.FastCopyFragment(n, signal.Length - n));
        }

        /// <summary>
        /// Full-rectifies <paramref name="signal"/> in-place.
        /// </summary>
        /// <param name="signal">Signal</param>
        public static void FullRectify(this DiscreteSignal signal)
        {
            for (var i = 0; i < signal.Length; i++)
            {
                if (signal[i] < 0)
                {
                    signal[i] = -signal[i];
                }
            }
        }

        /// <summary>
        /// Half-rectifies <paramref name="signal"/> in-place.
        /// </summary>
        /// <param name="signal">Signal</param>
        public static void HalfRectify(this DiscreteSignal signal)
        {
            for (var i = 0; i < signal.Length; i++)
            {
                if (signal[i] < 0)
                {
                    signal[i] = 0;
                }
            }
        }

        /// <summary>
        /// Normalizes <paramref name="signal"/> by its max absolute value (to range [-1, 1]).
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="bitsPerSample">Bit depth</param>
        public static void NormalizeMax(this DiscreteSignal signal, int bitsPerSample = 0)
        {
            var norm = 1 / signal.Samples.Max(s => Math.Abs(s));

            if (bitsPerSample > 0)
            {
                norm *= (float)(1 - 1 / Math.Pow(2, bitsPerSample));
            }

            signal.Amplify(norm);
        }

        /// <summary>
        /// Creates <see cref="ComplexDiscreteSignal"/> from <see cref="DiscreteSignal"/>. 
        /// Imaginary parts will be filled with zeros.
        /// </summary>
        /// <param name="signal">Real-valued signal</param>
        public static ComplexDiscreteSignal ToComplex(this DiscreteSignal signal)
        {
            return new ComplexDiscreteSignal(signal.SamplingRate, signal.Samples.ToDoubles());
        }
    }
}
