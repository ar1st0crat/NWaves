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
    ///    For all tasks users will most likely use real-valued DiscreteSignal.
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
        public virtual int SamplingRate { get; }

        /// <summary>
        /// Array or real parts of samples
        /// </summary>
        public virtual double[] Real { get; }

        /// <summary>
        /// Array or imaginary parts of samples
        /// </summary>
        public virtual double[] Imag { get; }

        /// <summary>
        /// Get real-valued signal containing magnitudes of complex-valued samples
        /// </summary>
        public DiscreteSignal Magnitude
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

                return new DiscreteSignal(SamplingRate, magnitude);
            }
        }

        /// <summary>
        /// Get real-valued signal containing phases of complex-valued samples
        /// </summary>
        public DiscreteSignal Phase
        {
            get
            {
                var real = Real;
                var imag = Imag;

                var magnitude = new double[real.Length];
                for (var i = 0; i < magnitude.Length; i++)
                {
                    magnitude[i] = Math.Atan(imag[i] / real[i]);
                }

                return new DiscreteSignal(SamplingRate, magnitude);
            }
        }

        /// <summary>
        /// The most efficient constructor for initializing complex signals
        /// </summary>
        /// <param name="samplingRate">Sampling rate of the signal</param>
        /// <param name="real">Array of real parts of the complex-valued signal</param>
        /// <param name="imag">Array of imaginary parts of the complex-valued signal</param>
        public ComplexDiscreteSignal(int samplingRate, double[] real, double[] imag = null)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;
            Real = FastCopy.EntireArray(real);

            // additional logic for imaginary part initialization

            if (imag != null)
            {
                if (imag.Length != real.Length)
                {
                    throw new ArgumentException("Arrays of real and imaginary parts have different size!");
                }

                Imag = FastCopy.EntireArray(imag);
            }
            else
            {
                Imag = new double[real.Length];
            }
        }

        /// <summary>
        /// Constructor for initializing complex signals with any double enumerables
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
        /// 
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
        /// Create a copy of complex signal
        /// </summary>
        /// <returns>New copied signal</returns>
        public ComplexDiscreteSignal Copy()
        {
            return new ComplexDiscreteSignal(SamplingRate, Real, Imag);
        }

        /// <summary>
        /// Indexer works only with array of real parts of samples. Use it with caution.
        /// </summary>
        public virtual double this[int index]
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
        public virtual ComplexDiscreteSignal this[int startPos, int endPos]
        {
            get
            {
                var rangeLength = endPos - startPos;

                if (rangeLength <= 0)
                {
                    throw new ArgumentException("Wrong index range!");
                }

                return new ComplexDiscreteSignal(SamplingRate,
                                    FastCopy.ArrayFragment(Real, rangeLength, startPos),
                                    FastCopy.ArrayFragment(Imag, rangeLength, startPos));
            }
        }

        /// <summary>
        /// Overloaded operator+ for signals concatenates these signals
        /// </summary>
        /// <param name="s1">First complex signal</param>
        /// <param name="s2">Second complex signal</param>
        /// <returns></returns>
        public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s1, ComplexDiscreteSignal s2)
        {
            return s1.Concatenate(s2);
        }

        /// <summary>
        /// Overloaded operator+ for some number performs signal delay by this number
        /// </summary>
        /// <param name="s">Complex signal</param>
        /// <param name="delay">Number of samples</param>
        /// <returns></returns>
        public static ComplexDiscreteSignal operator +(ComplexDiscreteSignal s, int delay)
        {
            return s.Delay(delay);
        }

        /// <summary>
        /// Overloaded operator* repeats signal several times
        /// </summary>
        /// <param name="s">Complex signal</param>
        /// <param name="times">Number of times</param>
        /// <returns></returns>
        public static ComplexDiscreteSignal operator *(ComplexDiscreteSignal s, int times)
        {
            return s.Repeat(times);
        }
    }
}
