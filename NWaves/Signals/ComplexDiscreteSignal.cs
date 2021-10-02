using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NWaves.Utils;

namespace NWaves.Signals
{
    /// <summary>
    /// Base class for finite complex-valued discrete-time signals. 
    /// Finite complex DT signal is stored as two arrays of data (real parts and imaginary parts) sampled at certain sampling rate. 
    /// See also <see cref="ComplexDiscreteSignalExtensions"/> for extra functionality of complex DT signals.
    /// </summary>
    public class ComplexDiscreteSignal
    {
        /// <summary>
        /// Gets sampling rate (number of samples per one second).
        /// </summary>
        public int SamplingRate { get; }

        /// <summary>
        /// Gets the real parts of complex-valued samples.
        /// </summary>
        public double[] Real { get; }

        /// <summary>
        /// Gets the imaginary parts of complex-valued samples.
        /// </summary>
        public double[] Imag { get; }

        /// <summary>
        /// Gets the length of the signal.
        /// </summary>
        public int Length => Real.Length;

        /// <summary>
        /// The most efficient constructor for initializing complex discrete signals. 
        /// By default, it just wraps <see cref="ComplexDiscreteSignal"/> 
        /// around arrays <paramref name="real"/> and <paramref name="imag"/> (without copying).
        /// If a new memory should be allocated for signal data, set <paramref name="allocateNew"/> to true.
        /// </summary>
        /// <param name="samplingRate">Sampling rate of the signal</param>
        /// <param name="real">Array of real parts of the complex-valued signal</param>
        /// <param name="imag">Array of imaginary parts of the complex-valued signal</param>
        /// <param name="allocateNew">Set to true if new memory should be allocated for data</param>
        public ComplexDiscreteSignal(int samplingRate, double[] real, double[] imag = null, bool allocateNew = false)
        {
            Guard.AgainstNonPositive(samplingRate, "Sampling rate");

            SamplingRate = samplingRate;
            Real = allocateNew ? real.FastCopy() : real;

            // additional logic for imaginary part initialization

            if (imag != null)
            {
                Guard.AgainstInequality(real.Length, imag.Length, "Number of real parts", "number of imaginary parts");

                Imag = allocateNew ? imag.FastCopy() : imag;
            }
            else
            {
                Imag = new double[real.Length];
            }
        }

        /// <summary>
        /// Constructs complex signal from collections of <paramref name="real"/> and <paramref name="imag"/> parts.
        /// </summary>
        /// <param name="samplingRate">Sampling rate of the signal</param>
        /// <param name="real">Array of real parts of the complex-valued signal</param>
        /// <param name="imag">Array of imaginary parts of the complex-valued signal</param>
        public ComplexDiscreteSignal(int samplingRate, IEnumerable<double> real, IEnumerable<double> imag = null)
            : this(samplingRate, real.ToArray(), imag?.ToArray())
        {
        }

        /// <summary>
        /// Constructs signal from collection of <paramref name="samples"/> sampled at <paramref name="samplingRate"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="samples">Collection of complex-valued samples</param>
        public ComplexDiscreteSignal(int samplingRate, IEnumerable<Complex> samples)
            : this(samplingRate, samples.Select(s => s.Real), samples.Select(s => s.Imaginary))
        {
        }

        /// <summary>
        /// Constructs signal of given <paramref name="length"/> filled with specified values.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="length">Number of samples</param>
        /// <param name="real">Value of each sample</param>
        /// <param name="imag">Value of each sample</param>
        public ComplexDiscreteSignal(int samplingRate, int length, double real = 0.0, double imag = 0.0)
        {
            Guard.AgainstNonPositive(samplingRate, "Sampling rate");

            SamplingRate = samplingRate;

            var reals = new double[length];
            var imags = new double[length];
            for (var i = 0; i < length; i++)
            {
                reals[i] = real;
                imags[i] = imag;
            }
            Real = reals;
            Imag = imags;
        }

        /// <summary>
        /// Constructs signal from collection of integer <paramref name="samples"/> sampled at given <paramref name="samplingRate"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="samples">Collection of integer samples</param>
        /// <param name="normalizeFactor">Each sample will be divided by this value</param>
        public ComplexDiscreteSignal(int samplingRate, IEnumerable<int> samples, double normalizeFactor = 1.0)
        {
            Guard.AgainstNonPositive(samplingRate, "Sampling rate");

            SamplingRate = samplingRate;

            var intSamples = samples.ToArray();
            var realSamples = new double[intSamples.Length];
            
            for (var i = 0; i < intSamples.Length; i++)
            {
                realSamples[i] = intSamples[i] / normalizeFactor;
            }

            Real = realSamples;
            Imag = new double[intSamples.Length];
        }

        /// <summary>
        /// Creates deep copy of the signal.
        /// </summary>
        public ComplexDiscreteSignal Copy()
        {
            return new ComplexDiscreteSignal(SamplingRate, Real, Imag, allocateNew: true);
        }

        /// <summary>
        /// Sample indexer. Works only with array of real parts of samples. Use it with caution.
        /// </summary>
        public double this[int index]
        {
            get => Real[index];
            set => Real[index] = value;
        }

        /// <summary>
        /// Creates the slice of the signal: 
        /// <code>
        ///     var middle = signal[900, 1200];
        /// </code>
        /// </summary>
        /// <param name="startPos">Index of the first sample (inclusive)</param>
        /// <param name="endPos">Index of the last sample (exclusive)</param>
        public ComplexDiscreteSignal this[int startPos, int endPos]
        {
            get
            {
                Guard.AgainstInvalidRange(startPos, endPos, "Left index", "Right index");

                var rangeLength = endPos - startPos;

                return new ComplexDiscreteSignal(SamplingRate,
                                    Real.FastCopyFragment(rangeLength, startPos),
                                    Imag.FastCopyFragment(rangeLength, startPos));
            }
        }

        /// <summary>
        /// Gets the magnitudes of complex-valued samples.
        /// </summary>
        public double[] Magnitude
        {
            get
            {
                var real = Real;
                var imag = Imag;

                var magnitude = new double[real.Length];
                for (var i = 0; i < magnitude.Length; i++)
                {
                    magnitude[i] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
                }

                return magnitude;
            }
        }

        /// <summary>
        /// Gets the power (squared magnitudes) of complex-valued samples.
        /// </summary>
        public double[] Power
        {
            get
            {
                var real = Real;
                var imag = Imag;

                var magnitude = new double[real.Length];
                for (var i = 0; i < magnitude.Length; i++)
                {
                    magnitude[i] = real[i] * real[i] + imag[i] * imag[i];
                }

                return magnitude;
            }
        }

        /// <summary>
        /// Gets the phases of complex-valued samples.
        /// </summary>
        public double[] Phase
        {
            get
            {
                var real = Real;
                var imag = Imag;

                var phase = new double[real.Length];
                for (var i = 0; i < phase.Length; i++)
                {
                    phase[i] = Math.Atan2(imag[i], real[i]);
                }

                return phase;
            }
        }

        /// <summary>
        /// Gets the unwrapped phases of complex-valued samples.
        /// </summary>
        public double[] PhaseUnwrapped => MathUtils.Unwrap(Phase);


        #region overloaded operators

        /// <summary>
        /// Creates new signal by superimposing signals <paramref name="s1"/> and <paramref name="s2"/>. 
        /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
        /// </summary>
        /// <param name="s1">First signal</param>
        /// <param name="s2">Second signal</param>
        public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s1, ComplexDiscreteSignal s2)
        {
            return s1.Superimpose(s2);
        }

        /// <summary>
        /// Creates new signal by adding <paramref name="constant"/> to signal <paramref name="s"/>.
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="constant">Constant to add to each sample</param>
        public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s, double constant)
        {
            return new ComplexDiscreteSignal(s.SamplingRate, s.Real.Select(x => x + constant));
        }

        /// <summary>
        /// Creates new signal by subtracting <paramref name="constant"/> from signal <paramref name="s"/>.
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="constant">Constant to subtract from each sample</param>
        public static ComplexDiscreteSignal operator -(ComplexDiscreteSignal s, double constant)
        {
            return new ComplexDiscreteSignal(s.SamplingRate, s.Real.Select(x => x - constant));
        }

        /// <summary>
        /// Creates new signal by multiplying <paramref name="s"/> by <paramref name="coeff"/> (amplification/attenuation).
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="coeff">Amplification/attenuation coefficient</param>
        public static ComplexDiscreteSignal operator *(ComplexDiscreteSignal s, float coeff)
        {
            var signal = s.Copy();
            signal.Amplify(coeff);
            return signal;
        }

        #endregion
    }
}
