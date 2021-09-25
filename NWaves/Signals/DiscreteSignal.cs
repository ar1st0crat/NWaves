using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Utils;

namespace NWaves.Signals
{
    /// <summary>
    /// Base class for finite real-valued discrete-time signals. 
    /// In general, any finite DT signal is simply an array of data sampled at certain sampling rate. 
    /// See also <see cref="DiscreteSignalExtensions"/> for extra functionality of DT signals.
    /// </summary>
    public class DiscreteSignal
    {
        /// <summary>
        /// Gets sampling rate (number of samples per one second).
        /// </summary>
        public int SamplingRate { get; }

        /// <summary>
        /// Gets real-valued array of samples.
        /// </summary>
        public float[] Samples { get; }

        /// <summary>
        /// Gets the length of the signal.
        /// </summary>
        public int Length => Samples.Length;

        /// <summary>
        /// Gets the duration of the signal (in seconds).
        /// </summary>
        public double Duration => (double)Samples.Length / SamplingRate;

        /// <summary>
        /// The most efficient constructor for initializing discrete signals. 
        /// By default, it just wraps <see cref="DiscreteSignal"/> around <paramref name="samples"/> (without copying).
        /// If a new memory should be allocated for signal data, set <paramref name="allocateNew"/> to true.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="samples">Array of samples</param>
        /// <param name="allocateNew">Set to true if new memory should be allocated for signal data</param>
        public DiscreteSignal(int samplingRate, float[] samples, bool allocateNew = false)
        {
            Guard.AgainstNonPositive(samplingRate, "Sampling rate");

            SamplingRate = samplingRate;
            Samples = allocateNew ? samples.FastCopy() : samples;
        }

        /// <summary>
        /// Construct signal from collection of <paramref name="samples"/> sampled at <paramref name="samplingRate"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="samples">Collection of samples</param>
        public DiscreteSignal(int samplingRate, IEnumerable<float> samples)
            : this(samplingRate, samples?.ToArray())
        {
        }

        /// <summary>
        /// Construct signal of given <paramref name="length"/> filled with specified values.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="length">Number of samples</param>
        /// <param name="value">Value of each sample</param>
        public DiscreteSignal(int samplingRate, int length, float value = 0.0f)
        {
            Guard.AgainstNonPositive(samplingRate, "Sampling rate");

            SamplingRate = samplingRate;

            var samples = new float[length];
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = value;
            }

            Samples = samples;
        }

        /// <summary>
        /// Construct signal from collection of integer <paramref name="samples"/> sampled at given <paramref name="samplingRate"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="samples">Collection of integer samples</param>
        /// <param name="normalizeFactor">Each sample will be divided by this value</param>
        public DiscreteSignal(int samplingRate, IEnumerable<int> samples, float normalizeFactor = 1.0f)
        {
            Guard.AgainstNonPositive(samplingRate, "Sampling rate");

            SamplingRate = samplingRate;
            
            var intSamples = samples.ToArray();
            var floatSamples = new float[intSamples.Length];
            for (var i = 0; i < intSamples.Length; i++)
            {
                floatSamples[i] = intSamples[i] / normalizeFactor;
            }

            Samples = floatSamples;
        }

        /// <summary>
        /// Generate unit impulse of given <paramref name="length"/> sampled at given <paramref name="samplingRate"/>.
        /// </summary>
        /// <param name="length">Length of unit impulse</param>
        /// <param name="samplingRate">Sampling rate</param>
        public static DiscreteSignal Unit(int length, int samplingRate = 1)
        {
            var unit = new float[length];
            unit[0] = 1;

            return new DiscreteSignal(samplingRate, unit);
        }

        /// <summary>
        /// Generate constant signal of given <paramref name="length"/> sampled at given <paramref name="samplingRate"/>.
        /// </summary>
        /// <param name="constant">Constant value</param>
        /// <param name="length">Length of constant signal</param>
        /// <param name="samplingRate">Sampling rate</param>
        public static DiscreteSignal Constant(float constant, int length, int samplingRate = 1)
        {
            return new DiscreteSignal(samplingRate, length, constant);
        }

        /// <summary>
        /// Create deep copy of the signal.
        /// </summary>
        public DiscreteSignal Copy()
        {
            return new DiscreteSignal(SamplingRate, Samples, true);
        }

        /// <summary>
        /// Sample indexer.
        /// </summary>
        /// <param name="index">Sample index</param>
        public float this[int index]
        {
            get => Samples[index];
            set => Samples[index] = value;
        }

        /// <summary>
        /// Create the slice of the signal: 
        /// <code>
        ///     var middle = signal[900, 1200];
        /// </code>
        /// </summary>
        /// <param name="startPos">Index of the first sample (inclusive)</param>
        /// <param name="endPos">Index of the last sample (exclusive)</param>
        public DiscreteSignal this[int startPos, int endPos]
        {
            get
            {
                Guard.AgainstInvalidRange(startPos, endPos, "Left index", "Right index");

                // Implementaion is LINQ-less, since Skip() would be less efficient:
                //     return new DiscreteSignal(SamplingRate, Samples.Skip(startPos).Take(endPos - startPos));

                return new DiscreteSignal(SamplingRate, Samples.FastCopyFragment(endPos - startPos, startPos));
            }
        }

        #region overloaded operators

        /// <summary>
        /// Create new signal by superimposing signals <paramref name="s1"/> and <paramref name="s2"/>. 
        /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
        /// </summary>
        /// <param name="s1">First signal</param>
        /// <param name="s2">Second signal</param>
        public static DiscreteSignal operator +(DiscreteSignal s1, DiscreteSignal s2)
        {
            return s1.Superimpose(s2);
        }

        /// <summary>
        /// Create negated copy of signal <paramref name="s"/>.
        /// </summary>
        /// <param name="s">Signal</param>
        public static DiscreteSignal operator -(DiscreteSignal s)
        {
            return new DiscreteSignal(s.SamplingRate, s.Samples.Select(x => -x));
        }

        /// <summary>
        /// Subtract signal <paramref name="s2"/> from signal <paramref name="s1"/>. 
        /// If sizes are different then the smaller signal is broadcast to fit the size of the larger signal.
        /// </summary>
        /// <param name="s1">First signal</param>
        /// <param name="s2">Second signal</param>
        public static DiscreteSignal operator -(DiscreteSignal s1, DiscreteSignal s2)
        {
            return s1.Subtract(s2);
        }

        /// <summary>
        /// Create new signal by adding <paramref name="constant"/> to signal <paramref name="s"/>.
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="constant">Constant to add to each sample</param>
        public static DiscreteSignal operator +(DiscreteSignal s, float constant)
        {
            return new DiscreteSignal(s.SamplingRate, s.Samples.Select(x => x + constant));
        }

        /// <summary>
        /// Create new signal by subtracting <paramref name="constant"/> from signal <paramref name="s"/>.
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="constant">Constant to subtract from each sample</param>
        public static DiscreteSignal operator -(DiscreteSignal s, float constant)
        {
            return new DiscreteSignal(s.SamplingRate, s.Samples.Select(x => x - constant));
        }

        /// <summary>
        /// Create new signal by multiplying <paramref name="s"/> by <paramref name="coeff"/> (amplification/attenuation).
        /// </summary>
        /// <param name="s">Signal</param>
        /// <param name="coeff">Amplification/attenuation coefficient</param>
        public static DiscreteSignal operator *(DiscreteSignal s, float coeff)
        {
            var signal = s.Copy();
            signal.Amplify(coeff);
            return signal;
        }

        #endregion

        #region time-domain characteristics

        /// <summary>
        /// Compute energy of a signal fragment.
        /// </summary>
        /// <param name="startPos">Index of the first sample (inclusive)</param>
        /// <param name="endPos">Index of the last sample (exclusive)</param>
        public float Energy(int startPos, int endPos)
        {
            var total = 0.0f;
            for (var i = startPos; i < endPos; i++)
            {
                total += Samples[i] * Samples[i];
            }

            return total / (endPos - startPos);
        }

        /// <summary>
        /// Compute energy of entire signal.
        /// </summary>
        public float Energy() => Energy(0, Length);

        /// <summary>
        /// Compute RMS of a signal fragment.
        /// </summary>
        /// <param name="startPos">Index of the first sample (inclusive)</param>
        /// <param name="endPos">Index of the last sample (exclusive)</param>
        public float Rms(int startPos, int endPos)
        {
            return (float)Math.Sqrt(Energy(startPos, endPos));
        }

        /// <summary>
        /// Compute RMS of entire signal.
        /// </summary>
        public float Rms() => (float)Math.Sqrt(Energy(0, Length));

        /// <summary>
        /// Compute Zero-crossing rate of a signal fragment.
        /// </summary>
        /// <param name="startPos">Index of the first sample (inclusive)</param>
        /// <param name="endPos">Index of the last sample (exclusive)</param>
        public float ZeroCrossingRate(int startPos, int endPos)
        {
            const float disbalance = 1e-4f;

            var prevSample = Samples[startPos] + disbalance;

            var rate = 0;
            for (var i = startPos + 1; i < endPos; i++)
            {
                var sample = Samples[i] + disbalance;

                if ((sample >= 0) != (prevSample >= 0))
                {
                    rate++;
                }

                prevSample = sample;
            }

            return (float)rate / (endPos - startPos - 1);
        }

        /// <summary>
        /// Compute Zero-crossing rate of entire signal.
        /// </summary>
        public float ZeroCrossingRate() => ZeroCrossingRate(0, Length);

        /// <summary>
        /// Compute Shannon entropy of a signal fragment 
        /// (from bins distributed uniformly between the minimum and maximum values of samples).
        /// </summary>
        /// <param name="startPos">Index of the first sample (inclusive)</param>
        /// <param name="endPos">Index of the last sample (exclusive)</param>
        /// <param name="binCount">Number of bins</param>
        public float Entropy(int startPos, int endPos, int binCount = 32)
        {
            var len = endPos - startPos;

            if (len < binCount)
            {
                binCount = len;
            }

            var bins = new int[binCount+1];

            var min = Samples[0];
            var max = Samples[0];
            for (var i = startPos; i < endPos; i++)
            {
                var sample = Math.Abs(Samples[i]);

                if (sample < min)
                {
                    min = sample;
                }
                if (sample > max)
                {
                    max = sample;
                }
            }

            if (max - min < 1e-8f)
            {
                return 0;
            }

            var binLength = (max - min) / binCount;

            for (var i = startPos; i < endPos; i++)
            {
                bins[(int)((Math.Abs(Samples[i]) - min) / binLength)]++;
            }

            var entropy = 0.0;
            for (var i = 0; i < binCount; i++)
            {
                var p = (float) bins[i] / (endPos - startPos);

                if (p > 1e-8f)
                {
                    entropy += p * Math.Log(p, 2);
                }
            }

            return (float)(-entropy / Math.Log(binCount, 2));
        }

        /// <summary>
        /// Compute Shannon entropy of entire signal 
        /// (from bins distributed uniformly between the minimum and maximum values of samples).
        /// </summary>
        /// <param name="binCount">Number of bins</param>
        public float Entropy(int binCount = 32) => Entropy(0, Length, binCount);

        #endregion
    }
}
