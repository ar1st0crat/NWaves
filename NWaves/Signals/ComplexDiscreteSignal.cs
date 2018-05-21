using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Utils;

namespace NWaves.Signals
{
    /// <summary>
    /// Base class for finite complex-valued discrete-time signals.
    /// 
    /// Any finite complex DT signal is stored as two arrays of data (real parts and imaginary parts)
    /// sampled at certain sampling rate.
    /// 
    /// See also ComplexDiscreteSignalExtensions for additional functionality of complex DT signals.
    /// 
    /// Note.
    /// 1) I intentionally do not implement reusable code mechanisms (like generics or inheritance) 
    ///    for coding DiscreteSignals and ComplexDiscreteSignals. Also for better performance 
    ///    I did not use Complex type (instead we just work with 2 plain arrays).
    ///    The reason is that currently ComplexDiscreteSignal is more like a helper class used in DSP internals.
    ///    For all tasks users will most likely use real-valued DiscreteSignal or simply an array of samples.
    ///    However they can switch between complex and real-valued signals anytime.
    /// 
    /// 2) Method implementations are LINQ-less for better performance.
    /// 
    /// </summary>
    public class ComplexDiscreteSignal
    {
        /// <summary>
        /// Number of samples per unit of time (1 second)
        /// </summary>
        public int SamplingRate { get; }

        /// <summary>
        /// Array or real parts of samples
        /// </summary>
        public double[] Real { get; }

        /// <summary>
        /// Array or imaginary parts of samples
        /// </summary>
        public double[] Imag { get; }

        /// <summary>
        /// Length of the signal
        /// </summary>
        public int Length => Real.Length;

        /// <summary>
        /// The most efficient constructor for initializing complex signals
        /// </summary>
        /// <param name="samplingRate">Sampling rate of the signal</param>
        /// <param name="real">Array of real parts of the complex-valued signal</param>
        /// <param name="imag">Array of imaginary parts of the complex-valued signal</param>
        /// <param name="allocateNew">Set to true if new memory should be allocated for data</param>
        public ComplexDiscreteSignal(int samplingRate, double[] real, double[] imag = null, bool allocateNew = false)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;
            Real = allocateNew ? real.FastCopy() : real;

            // additional logic for imaginary part initialization

            if (imag != null)
            {
                if (imag.Length != real.Length)
                {
                    throw new ArgumentException("Arrays of real and imaginary parts have different size!");
                }

                Imag = allocateNew ? imag.FastCopy() : imag;
            }
            else
            {
                Imag = new double[real.Length];
            }
        }

        /// <summary>
        /// Constructor for initializing complex signals with any float enumerables
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="real"></param>
        /// <param name="imag"></param>
        public ComplexDiscreteSignal(int samplingRate, IEnumerable<double> real, IEnumerable<double> imag = null)
            : this(samplingRate, real.ToArray(), imag?.ToArray())
        {
        }

        /// <summary>
        /// Constructor creates the complex signal of specified length filled with specified values
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="length"></param>
        /// <param name="real"></param>
        /// <param name="imag"></param>
        public ComplexDiscreteSignal(int samplingRate, int length, double real = 0.0, double imag = 0.0)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

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
        /// Constructor for initializing complex signals with any integer enumerables
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="samples"></param>
        /// <param name="normalizeFactor"></param>
        public ComplexDiscreteSignal(int samplingRate, IEnumerable<int> samples, double normalizeFactor = 1.0)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

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
        /// Method for creating the deep copy of a complex signal
        /// </summary>
        /// <returns>New copied signal</returns>
        public ComplexDiscreteSignal Copy()
        {
            return new ComplexDiscreteSignal(SamplingRate, Real, Imag, allocateNew: true);
        }

        /// <summary>
        /// Indexer works only with array of real parts of samples. Use it with caution.
        /// </summary>
        public double this[int index]
        { 
            get { return Real[index]; }
            set { Real[index] = value; }
        }

        /// <summary>
        /// Slice the signal (Python-style)
        /// 
        ///     var middle = signal[900, 1200];
        /// 
        /// Implementaion is LINQ-less, since Skip() would be less efficient:
        ///                 
        ///     return new DiscreteSignal(SamplingRate, 
        ///                               Real.Skip(startPos).Take(endPos - startPos),
        ///                               Imag.Skip(startPos).Take(endPos - startPos));
        /// </summary>
        /// <param name="startPos">Position of the first sample</param>
        /// <param name="endPos">Position of the last sample (exclusive)</param>
        /// <returns>Slice of the signal</returns>
        /// <exception>Overflow possible if endPos is less than startPos</exception>
        public ComplexDiscreteSignal this[int startPos, int endPos]
        {
            get
            {
                var rangeLength = endPos - startPos;

                if (rangeLength <= 0)
                {
                    throw new ArgumentException("Wrong index range!");
                }

                return new ComplexDiscreteSignal(SamplingRate,
                                    Real.FastCopyFragment(rangeLength, startPos),
                                    Imag.FastCopyFragment(rangeLength, startPos));
            }
        }

        /// <summary>
        /// Get real-valued signal containing magnitudes of complex-valued samples
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
        /// Get real-valued signal containing squared magnitudes of complex-valued samples
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
        /// Get real-valued signal containing phases of complex-valued samples
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
        /// Get unwrapped phase
        /// </summary>
        public double[] PhaseUnwrapped => MathUtils.Unwrap(Phase);

        /// <summary>
        /// Get group delay of complex-valued samples
        /// </summary>
        public double[] GroupDelay
        {
            get
            {
                var phase = MathUtils.Unwrap(Phase);

                var gd = new double[phase.Length - 1];
                for (var i = 0; i < gd.Length; i++)
                {
                    gd[i] = (phase[i] - phase[i + 1]) * gd.Length / Math.PI;
                }

                // replace each outlier with averaged value of neigboring samples

                var diffThreshold = gd.Average(g => Math.Abs(g)) * 5;

                for (var i = 1; i < gd.Length - 1; i++)
                {
                    if (Math.Abs(gd[i]) > diffThreshold)
                    {
                        gd[i] = (gd[i - 1] + gd[i + 1]) / 2;
                    }
                }

                if (Math.Abs(gd[0]) > diffThreshold) gd[0] = gd[1];
                if (Math.Abs(gd[gd.Length - 1]) > diffThreshold) gd[gd.Length - 1] = gd[gd.Length - 2];

                return gd;
            }
        }

        /// <summary>
        /// Get phase delay of complex-valued samples
        /// </summary>
        public double[] PhaseDelay
        {
            get
            {
                var gd = GroupDelay;

                var pd = new double[gd.Length];
                var acc = 0.0;
                for (var i = 0; i < pd.Length; i++)     // integrate group delay
                {
                    acc += gd[i];
                    pd[i] = acc / (i + 1);
                }
                
                return pd;
            }
        }

        #region overloaded operators

        /// <summary>
        /// Overloaded + (superimpose signals)
        /// </summary>
        /// <param name="s1">Left signal</param>
        /// <param name="s2">Right signal</param>
        /// <returns>Superimposed signal</returns>
        public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s1, ComplexDiscreteSignal s2)
        {
            return s1.Superimpose(s2);
        }

        /// <summary>
        /// Overloaded + (add constant)
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="constant">Constant to add to each sample</param>
        /// <returns>Modified signal</returns>
        public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s, double constant)
        {
            return new ComplexDiscreteSignal(s.SamplingRate, s.Real.Select(x => x + constant));
        }

        /// <summary>
        /// Overloaded - (subtract constant)
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="constant">Constant to subtract from each sample</param>
        /// <returns>Modified signal</returns>
        public static ComplexDiscreteSignal operator -(ComplexDiscreteSignal s, double constant)
        {
            return new ComplexDiscreteSignal(s.SamplingRate, s.Real.Select(x => x - constant));
        }

        /// <summary>
        /// Overloaded * (signal amplification)
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="coeff">Amplification coefficient</param>
        /// <returns>Amplified signal</returns>
        public static ComplexDiscreteSignal operator *(ComplexDiscreteSignal s, float coeff)
        {
            var signal = s.Copy();
            signal.Amplify(coeff);
            return signal;
        }

        #endregion
    }
}
