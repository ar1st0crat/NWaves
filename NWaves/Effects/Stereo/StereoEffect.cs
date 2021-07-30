using NWaves.Filters.Base;
using NWaves.Signals;
using System;

namespace NWaves.Effects.Stereo
{
    /// <summary>
    /// Base class for stereo effects
    /// </summary>
    public abstract class StereoEffect : IMixable
    {
        /// <summary>
        /// Wet mix
        /// </summary>
        public virtual float Wet { get; set; } = 1f;

        /// <summary>
        /// Dry mix
        /// </summary>
        public virtual float Dry { get; set; } = 0f;

        /// <summary>
        /// Process two channels : [ input left , input right ] -> [ output left , output right ]
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public abstract void Process(ref float left, ref float right);

        /// <summary>
        /// Process mono channel : input sample -> [ output left , output right ]
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public virtual void Process(float sample, out float left, out float right)
        {
            left = right = sample;

            Process(ref left, ref right);
        }

        /// <summary>
        /// Process two channels (in blocks) : [ input left , input right ] -> [ output left , output right ]
        /// </summary>
        /// <param name="inputLeft">Input block of samples (left channel)</param>
        /// <param name="inputRight">Input block of samples (right channel)</param>
        /// <param name="outputLeft">Block of filtered samples (left channel)</param>
        /// <param name="outputRight">Block of filtered samples (right channel)</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        public virtual void Process(float[] inputLeft,
                                    float[] inputRight,
                                    float[] outputLeft,
                                    float[] outputRight,
                                    int count = 0,
                                    int inputPos = 0,
                                    int outputPos = 0)
        {
            if (count <= 0)
            {
                count = Math.Min(inputLeft.Length, inputRight.Length);
            }

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                outputLeft[m] = inputLeft[n];
                outputRight[m] = inputRight[n];

                Process(ref outputLeft[m], ref outputRight[m]);
            }
        }

        /// <summary>
        /// Process mono channel (in blocks) : [ input ] -> [ output left , output right ]
        /// </summary>
        /// <param name="input">Input block of samples (mono channel)</param>
        /// <param name="outputLeft">Block of filtered samples (left channel)</param>
        /// <param name="outputRight">Block of filtered samples (right channel)</param>
        /// <param name="count">Number of samples to filter</param>
        /// <param name="inputPos">Input starting position</param>
        /// <param name="outputPos">Output starting position</param>
        public virtual void Process(float[] input,
                                    float[] outputLeft,
                                    float[] outputRight,
                                    int count = 0,
                                    int inputPos = 0,
                                    int outputPos = 0)
        {
            if (count <= 0)
            {
                count = input.Length;
            }

            var endPos = inputPos + count;

            for (int n = inputPos, m = outputPos; n < endPos; n++, m++)
            {
                outputLeft[m] = outputRight[m] = input[n];

                Process(ref outputLeft[m], ref outputRight[m]);
            }
        }

        /// <summary>
        /// Offline processing
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Tuple [left signal, right signal]</returns>
        public virtual (DiscreteSignal, DiscreteSignal) ApplyTo(DiscreteSignal signal)
        {
            var sr = signal.SamplingRate;

            var left = new float[signal.Length];
            var right = new float[signal.Length];
            
            Process(signal.Samples, left, right);

            return (new DiscreteSignal(sr, left), new DiscreteSignal(sr, right));
        }

        /// <summary>
        /// Offline processing
        /// </summary>
        /// <param name="leftSignal">Input signal (left)</param>
        /// <param name="rightSignal">Input signal (right)</param>
        /// <returns>Tuple [left signal, right signal]</returns>
        public virtual (DiscreteSignal, DiscreteSignal) ApplyTo(DiscreteSignal leftSignal, DiscreteSignal rightSignal)
        {
            var srl = leftSignal.SamplingRate;
            var srr = rightSignal.SamplingRate;

            var left = new float[leftSignal.Length];
            var right = new float[rightSignal.Length];

            Process(leftSignal.Samples, rightSignal.Samples, left, right);

            return (new DiscreteSignal(srl, left), new DiscreteSignal(srr, right));
        }

        /// <summary>
        /// Reset effect
        /// </summary>
        public abstract void Reset();
    }
}
