using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals
{
    /// <summary>
    /// 
    /// Base class for finite discrete-time signals.
    /// 
    /// In general, any finite DT signal is simply an array of data sampled at certain sampling rate.
    /// 
    /// See also DiscreteSignalExtensions for additional functionality of DT signals.
    /// 
    /// Note. 
    /// In the earliest versions of NWaves there was also an ISignal interface, however it was refactored out.
    /// If there's a need, just inherit from this base class, all methods are virtual.
    /// 
    /// </summary>
    public class DiscreteSignal
    {
        /// <summary>
        /// 
        /// </summary>
        public virtual double[] Samples { get; }

        /// <summary>
        /// Number of samples per unit of time (1 second)
        /// </summary>
        public virtual int SamplingRate { get; }

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        protected DiscreteSignal()
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
            Samples = Enumerable.Repeat(value, length).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="samplingRate"></param>
        public DiscreteSignal(IEnumerable<double> samples, int samplingRate)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;
            Samples = samples.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DiscreteSignal Copy()
        {
            return new DiscreteSignal(Samples, SamplingRate);
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
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        /// <exception></exception>
        public virtual DiscreteSignal this[int startPos, int endPos] => 
            new DiscreteSignal(Samples.Skip(startPos).Take(endPos - startPos), SamplingRate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        public static DiscreteSignal operator +(DiscreteSignal s1, DiscreteSignal s2)
        {
            return s1.Concatenate(s2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static DiscreteSignal operator +(DiscreteSignal s, int delay)
        {
            return s.Delay(delay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        public static DiscreteSignal operator *(DiscreteSignal s, int times)
        {
            return s.Repeat(times);
        }
    }
}
