using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Utils;

namespace NWaves.Signals
{
    /// <summary>
    /// Base class for finite real-valued discrete-time signals.
    /// 
    /// In general, any finite DT signal is simply an array of data sampled at certain sampling rate.
    /// 
    /// See also DiscreteSignalExtensions for additional functionality of DT signals.
    /// 
    /// Note. 
    /// Method implementations are LINQ-less for better performance.
    /// 
    /// In the earliest versions of NWaves there was also an ISignal interface, however it was refactored out.
    /// If there's a need, just inherit from this base class, all methods are virtual.
    /// </summary>
    public class DiscreteSignal
    {
        /// <summary>
        /// Number of samples per unit of time (1 second)
        /// </summary>
        public virtual int SamplingRate { get; }

        /// <summary>
        /// Real-valued array of samples
        /// </summary>
        public virtual double[] Samples { get; }

        /// <summary>
        /// Length of the signal
        /// </summary>
        public virtual int Length => Samples.Length;

        /// <summary>
        /// The most efficient constructor for initializing discrete signals
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="samples"></param>
        /// <param name="allocateNew">Set to true if new memory should be allocated for data</param>
        public DiscreteSignal(int samplingRate, double[] samples, bool allocateNew = false)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;
            Samples = allocateNew ? FastCopy.EntireArray(samples) : samples;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="samples"></param>
        public DiscreteSignal(int samplingRate, IEnumerable<double> samples)
            : this(samplingRate, samples?.ToArray())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        public DiscreteSignal(int samplingRate, int length, double value = 0.0)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;

            var samples = new double[length];
            for (var i = 0; i < length; i++)
            {
                samples[i] = value;
            }

            Samples = samples;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="samples"></param>
        /// <param name="normalizeFactor"></param>
        public DiscreteSignal(int samplingRate, IEnumerable<int> samples, double normalizeFactor = 1.0)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;
            
            var intSamples = samples.ToArray();
            var doubleSamples = new double[intSamples.Length];
            for (var i = 0; i < intSamples.Length; i++)
            {
                doubleSamples[i] = intSamples[i] / normalizeFactor;
            }

            Samples = doubleSamples;
        }

        /// <summary>
        /// Method for creating deep copy of the signal
        /// </summary>
        /// <returns>Copy of the signal</returns>
        public DiscreteSignal Copy()
        {
            return new DiscreteSignal(SamplingRate, Samples, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual double this[int index]
        {
            get { return Samples[index]; }
            set { Samples[index] = value; }
        }

        /// <summary>
        /// Slice the signal (Python-style)
        /// 
        ///     var middle = signal[900, 1200];
        /// 
        /// Implementaion is LINQ-less, since Skip() would be less efficient:
        ///     return new DiscreteSignal(SamplingRate, Samples.Skip(startPos).Take(endPos - startPos));
        /// 
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns>Slice of the signal</returns>
        /// <exception>Overflow possible if endPos is less than startPos</exception>
        public virtual DiscreteSignal this[int startPos, int endPos]
        {
            get
            {
                var rangeLength = endPos - startPos;

                if (rangeLength <= 0)
                {
                    throw new ArgumentException("Wrong index range!");
                }

                return new DiscreteSignal(SamplingRate, FastCopy.ArrayFragment(Samples, rangeLength, startPos));
            }
        }

        #region time-domain characteristics

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPos">Starting sample</param>
        /// <param name="endPos">Ending sample (exclusive)</param>
        /// <returns></returns>
        public double Energy(int startPos, int endPos)
        {
            var total = 0.0;
            for (var i = startPos; i < endPos; i++)
            {
                total += Samples[i] * Samples[i];
            }

            return total / (endPos - startPos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Energy()
        {
            return Energy(0, Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public double Rms(int startPos, int endPos)
        {
            return Math.Sqrt(Energy(startPos, endPos));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Rms()
        {
            return Math.Sqrt(Energy(0, Length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double ZeroCrossingRate(int startPos, int endPos)
        {
            var rate = 0;
            for (var i = startPos + 1; i < endPos; i++)
            {
                if ((Samples[i - 1] >= 0) != (Samples[i] >= 0))
                {
                    rate++;
                }
            }

            return (double)rate / (endPos - startPos - 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double ZeroCrossingRate()
        {
            return ZeroCrossingRate(0, Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Entropy(int startPos, int endPos)
        {
            var sum = 0.0;
            for (var i = startPos; i < endPos; i++)
            {
                sum += Math.Abs(Samples[i]);
            }

            var entropy = 0.0;
            for (var i = startPos; i < endPos; i++)
            {
                var p = Math.Abs(Samples[i]) / sum;
                entropy -= p * Math.Log(p + double.Epsilon, 2);
            }

            return entropy;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double Entropy()
        {
            return Entropy(0, Length);
        }

        #endregion

        #region overloaded operators

        /// <summary>
        /// Overloaded + (signal concatentaion)
        /// </summary>
        /// <param name="s1">Left signal</param>
        /// <param name="s2">Right signal</param>
        /// <returns>Concatenated signal</returns>
        public static DiscreteSignal operator +(DiscreteSignal s1, DiscreteSignal s2)
        {
            return s1.Concatenate(s2);
        }

        /// <summary>
        /// Overloaded + (signal delay late)
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="delay">Number of samples to delay</param>
        /// <returns>Delayed signal</returns>
        public static DiscreteSignal operator +(DiscreteSignal s, int delay)
        {
            return s.Delay(delay);
        }

        /// <summary>
        /// Overloaded - (signal delay early)
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="delay">Number of samples to delay</param>
        /// <returns></returns>
        public static DiscreteSignal operator -(DiscreteSignal s, int delay)
        {
            return s.Delay(-delay);
        }

        /// <summary>
        /// Overloaded * (signal repetition)
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="times">Repeat times</param>
        /// <returns>Repeated signal</returns>
        public static DiscreteSignal operator *(DiscreteSignal s, int times)
        {
            return s.Repeat(times);
        }

        #endregion
    }
}
